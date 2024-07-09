using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.binary.dependencies;
using schema.util.asserts;
using schema.util.symbols;
using schema.util.text;

namespace schema.binary.text {
  public class BinarySchemaReaderGenerator {
    public const string READER = "br";

    public string Generate(IBinarySchemaContainer container) {
      var typeSymbol = container.TypeSymbol;

      var sb = new StringBuilder();
      using var sw = new SourceWriter(new StringWriter(sb));

      {
        var dependencies = new List<string> { "System", "schema.binary" };
        if (container.DependsOnSequenceImports()) {
          dependencies.Add("schema.util.sequences");
        }

        if (container.DependsOnSchemaAttributes()) {
          dependencies.Add("schema.binary.attributes");
        }

        if (container.DependsOnCollectionsImports()) {
          dependencies.Add("System.Collections.Generic");
        }

        dependencies.Sort(StringComparer.Ordinal);
        foreach (var dependency in dependencies) {
          sw.WriteLine($"using {dependency};");
        }

        sw.WriteLine("");
      }

      sw.WriteNamespaceAndParentTypeBlocks(
          typeSymbol,
          () => {
            sw.EnterBlock(
                typeSymbol.GetQualifiersAndNameAndGenericParametersFor());

            sw.EnterBlock($"public void Read(IBinaryReader {READER})");
            {
              var hasLocalPositions = container.LocalPositions;
              if (hasLocalPositions) {
                sw.WriteLine($"{READER}.PushLocalSpace();");
              }

              var hasEndianness = container.Endianness != null;
              if (hasEndianness) {
                sw.WriteLine(
                    $"{READER}.PushContainerEndianness({SchemaGeneratorUtil.GetEndiannessName(container.Endianness.Value)});");
              }

              foreach (var member in container.Members) {
                if (member is ISchemaValueMember valueMember) {
                  BinarySchemaReaderGenerator.ReadValueMember_(
                      sw,
                      typeSymbol,
                      valueMember);
                } else if (member is ISchemaMethodMember) {
                  sw.WriteLine($"this.{member.Name}({READER});");
                }
              }

              if (hasEndianness) {
                sw.WriteLine($"{READER}.PopEndianness();");
              }

              if (hasLocalPositions) {
                sw.WriteLine($"{READER}.PopLocalSpace();");
              }
            }
            sw.ExitBlock();

            // TODO: Handle fancier cases here

            // type
            sw.ExitBlock();
          });

      var generatedCode = sb.ToString();
      return generatedCode;
    }

    private static void ReadValueMember_(
        ISourceWriter sw,
        ITypeSymbol sourceSymbol,
        ISchemaValueMember member) {
      if (member.IsPosition) {
        if (member.MemberType.IsReadOnly) {
          sw.WriteLine($"{READER}.AssertPosition(this.{member.Name});");
        } else {
          sw.WriteLine($"this.{member.Name} = {READER}.Position;");
        }

        return;
      }

      if (member.IsSkipped) {
        return;
      }

      // TODO: How to handle both offset & if boolean together?

      var offset = member.Offset;
      if (offset != null) {
        var nullValue = offset.NullValue;
        var readBlockPrefix = "";
        if (nullValue != null) {
          sw.EnterBlock($"if (this.{offset.OffsetName.Name} == {nullValue})")
            .WriteLine($"this.{member.Name} = null;")
            .ExitBlock();

          readBlockPrefix = "else";
        }

        sw.EnterBlock(readBlockPrefix)
          .WriteLine($"var tempLocation = {READER}.Position;")
          .WriteLine(
              $"{READER}.Position = this.{offset.OffsetName.Name};");
      }

      var ifBoolean = member.IfBoolean;
      var immediateIfBoolean =
          ifBoolean?.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE;
      if (immediateIfBoolean) {
        sw.EnterBlock();
      }

      if (ifBoolean != null) {
        if (ifBoolean.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE) {
          var booleanNumberType = ifBoolean.ImmediateBooleanType.AsNumberType();
          var booleanPrimitiveType = booleanNumberType.AsPrimitiveType();
          var booleanPrimitiveLabel =
              SchemaGeneratorUtil.GetPrimitiveLabel(booleanPrimitiveType);
          sw.WriteLine(
                $"var b = {READER}.Read{booleanPrimitiveLabel}() != 0;")
            .EnterBlock("if (b)");
        } else {
          sw.EnterBlock($"if (this.{ifBoolean.OtherMember.Name})");
        }

        if (member.MemberType is not IPrimitiveMemberType &&
            member.MemberType is not IContainerMemberType &&
            member.MemberType is not ISequenceMemberType {
                SequenceTypeInfo.SequenceType: SequenceType
                    .MUTABLE_ARRAY
            }) {
          sw.WriteLine(
              $"this.{member.Name} = new {sourceSymbol.GetQualifiedNameFromCurrentSymbol(member.MemberType.TypeSymbol.AsNonNullable())}();");
        }
      }

      var memberType = member.MemberType;
      if (memberType is IGenericMemberType genericMemberType) {
        memberType = genericMemberType.ConstraintType;
      }

      switch (memberType) {
        case IPrimitiveMemberType: {
          BinarySchemaReaderGenerator.ReadPrimitive_(
              sw,
              sourceSymbol,
              member);
          break;
        }
        case IStringType: {
          BinarySchemaReaderGenerator.ReadString_(sw, member);
          break;
        }
        case IContainerMemberType containerMemberType: {
          BinarySchemaReaderGenerator.ReadContainer_(sw,
            sourceSymbol,
            containerMemberType,
            member);
          break;
        }
        case ISequenceMemberType: {
          BinarySchemaReaderGenerator.ReadArray_(sw, sourceSymbol, member);
          break;
        }
        default: {
          // Anything that makes it down here probably isn't meant to be read.
          throw new NotImplementedException();
        }
      }

      if (ifBoolean != null) {
        sw.ExitBlock()
          .EnterBlock("else")
          .WriteLine($"this.{member.Name} = null;")
          .ExitBlock();
        if (immediateIfBoolean) {
          sw.ExitBlock();
        }
      }

      if (offset != null) {
        sw.WriteLine($"{READER}.Position = tempLocation;")
          .ExitBlock();
      }
    }

    private static void Align_(
        ISourceWriter sw,
        ISchemaValueMember member) {
      var align = member.Align;
      if (align == null) {
        return;
      }

      var valueName = align.Method switch {
          AlignSourceType.CONST        => $"{align.ConstAlign}",
          AlignSourceType.OTHER_MEMBER => $"{align.OtherMember.Name}"
      };
      sw.WriteLine($"{READER}.Align({valueName});");
    }

    private static void HandleMemberEndianness_(
        ISourceWriter sw,
        ISchemaValueMember member,
        Action handler) {
      var hasEndianness = member.Endianness != null;
      if (hasEndianness) {
        sw.WriteLine(
            $"{READER}.PushMemberEndianness({SchemaGeneratorUtil.GetEndiannessName(member.Endianness.Value)});");
      }

      BinarySchemaReaderGenerator.Align_(sw, member);

      handler();

      if (hasEndianness) {
        sw.WriteLine($"{READER}.PopEndianness();");
      }
    }

    private static void ReadPrimitive_(
        ISourceWriter sw,
        ITypeSymbol sourceSymbol,
        ISchemaValueMember member) {
      var primitiveType =
          Asserts.CastNonnull(member.MemberType as IPrimitiveMemberType);

      HandleMemberEndianness_(
          sw,
          member,
          () => {
            if (!primitiveType.IsReadOnly) {
              sw.WriteLine(
                  $"this.{member.Name} = {GetReadPrimitiveText_(sourceSymbol, primitiveType)};");
            } else {
              sw.WriteLine($"{GetAssertPrimitiveText_(
                  primitiveType,
                  $"this.{member.Name}")};");
            }
          });
    }

    private static void ReadString_(
        ISourceWriter sw,
        ISchemaValueMember member) {
      HandleMemberEndianness_(
          sw,
          member,
          () => {
            var stringType =
                Asserts.CastNonnull(
                    member.MemberType as IStringType);

            var encodingType = "";
            if (stringType.EncodingType != StringEncodingType.ASCII) {
              encodingType = stringType.EncodingType switch {
                  StringEncodingType.UTF8 => "StringEncodingType.UTF8",
                  StringEncodingType.UTF16 => "StringEncodingType.UTF16",
                  StringEncodingType.UTF32 => "StringEncodingType.UTF32",
                  _ => throw new ArgumentOutOfRangeException()
              };
            }

            var encodingTypeWithComma =
                encodingType.Length > 0 ? $"{encodingType}, " : "";

            if (stringType.IsReadOnly) {
              if (stringType.LengthSourceType ==
                  StringLengthSourceType.NULL_TERMINATED) {
                sw.WriteLine(
                    $"{READER}.AssertStringNT({encodingTypeWithComma}this.{member.Name});");
              } else {
                sw.WriteLine(
                    $"{READER}.AssertString({encodingTypeWithComma}this.{member.Name});");
              }

              return;
            }

            if (stringType.LengthSourceType ==
                StringLengthSourceType.NULL_TERMINATED) {
              sw.WriteLine(
                  $"this.{member.Name} = {READER}.ReadStringNT({encodingType});");
              return;
            }

            var readMethod = stringType.IsNullTerminated
                ? "ReadStringNT"
                : "ReadString";

            if (stringType.LengthSourceType == StringLengthSourceType.CONST) {
              sw.WriteLine(
                  $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}{stringType.ConstLength});");
              return;
            }

            if (stringType.LengthSourceType ==
                StringLengthSourceType.IMMEDIATE_VALUE) {
              var readType =
                  SchemaGeneratorUtil.GetIntLabel(
                      stringType.ImmediateLengthType);
              sw.EnterBlock()
                .WriteLine($"var l = {READER}.Read{readType}();")
                .WriteLine(
                    $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}l);")
                .ExitBlock();
              return;
            }

            if (stringType.LengthSourceType ==
                StringLengthSourceType.OTHER_MEMBER) {
              sw.WriteLine(
                  $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}{stringType.LengthMember.Name});");
              return;
            }

            // TODO: Handle more cases
            throw new NotImplementedException();
          });
    }

    private static void ReadContainer_(
        ISourceWriter sw,
        ITypeSymbol sourceSymbol,
        IContainerMemberType containerMemberType,
        ISchemaValueMember member) {
      // TODO: Do value types need to be handled differently?
      var memberName = member.Name;
      if (containerMemberType.IsChild) {
        sw.WriteLine($"this.{memberName}.Parent = this;");
      }

      HandleMemberEndianness_(
          sw,
          member,
          () => {
            var isNullable = containerMemberType.TypeInfo.IsNullable;
            var isStruct = containerMemberType.TypeSymbol.IsStruct();

            var qualifiedTypeName =
                SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                    sourceSymbol,
                    containerMemberType.TypeSymbol.AsNonNullable());

            if (isNullable) {
              sw.WriteLine(
                  $"this.{memberName} = {READER}.ReadNew<{qualifiedTypeName}>();");
            } else {
              if (!isStruct) {
                sw.WriteLine($"this.{memberName}.Read({READER});");
              } else {
                sw.EnterBlock()
                  .WriteLine($"var value = this.{memberName};")
                  .WriteLine($"value.Read({READER});")
                  .WriteLine($"this.{memberName} = value;")
                  .ExitBlock();
              }
            }
          });
    }

    private static void ReadArray_(
        ISourceWriter sw,
        ITypeSymbol sourceSymbol,
        ISchemaValueMember member) {
      var arrayType =
          Asserts.CastNonnull(member.MemberType as ISequenceMemberType);
      var sequenceType = arrayType.SequenceTypeInfo.SequenceType;
      // TODO: Not valid for sequences (yet?)
      if (arrayType.LengthSourceType ==
          SequenceLengthSourceType.UNTIL_END_OF_STREAM) {
        var qualifiedElementName =
            SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                sourceSymbol,
                arrayType.ElementType.TypeSymbol);

        var memberAccessor = $"this.{member.Name}";

        var isArray = sequenceType == SequenceType.MUTABLE_ARRAY;
        {
          if (isArray &&
              arrayType.ElementType is IPrimitiveMemberType
                  primitiveElementType &&
              SizeUtil.TryGetSizeOfType(arrayType.ElementType,
                                        out var size)) {
            var remainingLengthAccessor =
                $"{READER}.Length - {READER}.Position";
            var readCountAccessor = size == 1
                ? remainingLengthAccessor
                : $"({remainingLengthAccessor}) / {size}";

            // Primitives that don't need to be cast are the easiest to read.
            if (!primitiveElementType.UseAltFormat) {
              var label =
                  SchemaGeneratorUtil.GetPrimitiveLabel(
                      primitiveElementType.PrimitiveType);
              sw.WriteLine(
                  $"{memberAccessor} = {READER}.Read{label}s({readCountAccessor});");
            } else {
              sw.WriteLine(
                    $"{memberAccessor} = new {qualifiedElementName}[{readCountAccessor}];")
                .EnterBlock(
                    $"for (var i = 0; i < {memberAccessor}.Length; ++i)")
                .WriteLine(
                    $"{memberAccessor}[i] = {GetReadPrimitiveText_(sourceSymbol, primitiveElementType)};")
                .ExitBlock();
            }

            return;
          }
        }

        sw.EnterBlock();
        if (!isArray) {
          sw.WriteLine($"{memberAccessor}.Clear();");
        }

        var target = isArray ? "temp" : $"this.{member.Name}";

        if (isArray) {
          sw.WriteLine(
              $"var {target} = new List<{qualifiedElementName}>();");
        }

        {
          sw.EnterBlock($"while (!{READER}.Eof)");
          {
            var elementType = arrayType.ElementType;
            if (elementType is IGenericMemberType genericElementType) {
              elementType = genericElementType.ConstraintType;
            }

            if (elementType is IPrimitiveMemberType primitiveElementType) {
              sw.WriteLine(
                  $"{target}.Add({GetReadPrimitiveText_(sourceSymbol, primitiveElementType)});");
            } else if
                (elementType is IContainerMemberType containerElementType) {
              sw.WriteLine(
                  $"var e = new {SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                      sourceSymbol,
                      arrayType.ElementType.TypeSymbol.AsNonNullable())}();");

              if (containerElementType.IsChild) {
                sw.WriteLine("e.Parent = this;");
              }

              sw.WriteLine($"e.Read({READER});");
              sw.WriteLine($"{target}.Add(e);");
            }
          }
          sw.ExitBlock();
        }

        if (isArray) {
          sw.WriteLine($"this.{member.Name} = {target}.ToArray();");
        }

        sw.ExitBlock();

        return;
      } else if (arrayType.LengthSourceType !=
                 SequenceLengthSourceType.READ_ONLY) {
        var isImmediate =
            arrayType.LengthSourceType ==
            SequenceLengthSourceType.IMMEDIATE_VALUE;

        var lengthName = arrayType.LengthSourceType switch {
            SequenceLengthSourceType.IMMEDIATE_VALUE => "c",
            SequenceLengthSourceType.OTHER_MEMBER =>
                $"this.{arrayType.LengthMember!.Name}",
            SequenceLengthSourceType.CONST_LENGTH =>
                $"{arrayType.ConstLength}",
        };

        var castText = "";
        if ((isImmediate &&
             !arrayType.ImmediateLengthType.CanBeStoredInAnInt32()) ||
            (arrayType.LengthSourceType ==
             SequenceLengthSourceType.OTHER_MEMBER &&
             !(arrayType.LengthMember.MemberType as IPrimitiveMemberType)!
              .PrimitiveType.AsIntegerType()
              .CanBeStoredInAnInt32())) {
          castText = "(int) ";
        }

        if (isImmediate) {
          var readType = SchemaGeneratorUtil.GetIntLabel(
              arrayType.ImmediateLengthType);
          sw.EnterBlock()
            .WriteLine($"var {lengthName} = {READER}.Read{readType}();");
        }

        var inPlace =
            arrayType.SequenceTypeInfo.SequenceType ==
            SequenceType.MUTABLE_LIST ||
            arrayType.SequenceTypeInfo is {
                SequenceType: SequenceType.MUTABLE_SEQUENCE,
                IsLengthConst: false
            };
        if (inPlace) {
          sw.WriteLine(
              $"SequencesUtil.ResizeSequenceInPlace(this.{member.Name}, {castText}{lengthName});");
        } else {
          sw.WriteLine(
              $"this.{member.Name} = SequencesUtil.CloneAndResizeSequence(this.{member.Name}, {castText}{lengthName});");
        }

        if (isImmediate) {
          sw.ExitBlock();
        }
      }

      BinarySchemaReaderGenerator.ReadIntoArray_(sw, sourceSymbol, member);
    }

    private static void ReadIntoArray_(
        ISourceWriter sw,
        ITypeSymbol sourceSymbol,
        ISchemaValueMember member) {
      HandleMemberEndianness_(
          sw,
          member,
          () => {
            var sequenceMemberType =
                Asserts.CastNonnull(member.MemberType as ISequenceMemberType);
            var sequenceTypeInfo = sequenceMemberType.SequenceTypeInfo;
            var sequenceType = sequenceTypeInfo.SequenceType;

            if (sequenceType.IsISequence()) {
              sw.WriteLine($"this.{member.Name}.Read({READER});");
              return;
            }

            var elementType = sequenceMemberType.ElementType;
            if (elementType is IGenericMemberType
                genericElementType) {
              elementType =
                  genericElementType.ConstraintType;
            }

            if (elementType is IPrimitiveMemberType primitiveElementType) {
              // Primitives that don't need to be cast are the easiest to read.
              if (!primitiveElementType.UseAltFormat &&
                  sequenceType.IsArray()) {
                var label = SchemaGeneratorUtil.GetPrimitiveLabel(
                    primitiveElementType.PrimitiveType);
                if (!primitiveElementType.IsReadOnly) {
                  sw.WriteLine(
                      $"{READER}.Read{label}s(this.{member.Name});");
                } else {
                  sw.WriteLine(
                      $"{READER}.Assert{label}s(this.{member.Name});");
                }

                return;
              }

              // Primitives that *do* need to be cast have to be read individually.
              if (!primitiveElementType.IsReadOnly) {
                var arrayLengthName = sequenceTypeInfo.LengthName;

                sw.EnterBlock(
                      $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)")
                  .WriteLine(
                      $"this.{member.Name}[i] = {GetReadPrimitiveText_(sourceSymbol, primitiveElementType)};")
                  .ExitBlock();
              } else {
                sw.EnterBlock(
                      $"foreach (var e in this.{member.Name})")
                  .WriteLine(
                      $"{GetAssertPrimitiveText_(
                          primitiveElementType,
                          "e")};")
                  .ExitBlock();
              }

              return;
            }

            if (elementType is IContainerMemberType containerElementType) {
              if (!containerElementType.TypeSymbol.IsStruct()) {
                sw.EnterBlock(
                    $"foreach (var e in this.{member.Name})");

                if (containerElementType.IsChild) {
                  sw.WriteLine("e.Parent = this;");
                }

                sw.WriteLine($"e.Read({READER});");
                sw.ExitBlock();
              } else {
                var arrayLengthName = sequenceTypeInfo.LengthName;
                sw.EnterBlock(
                    $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)");
                sw.WriteLine(
                    $"var e = this.{member.Name}[i];");

                if (containerElementType.IsChild) {
                  sw.WriteLine("e.Parent = this;");
                }

                sw.WriteLine($"e.Read({READER});");
                sw.WriteLine(
                    $"this.{member.Name}[i] = e;");

                sw.ExitBlock();
              }

              return;
            }

            // Anything that makes it down here probably isn't meant to be read.
            throw new NotImplementedException();
          });
    }

    private static string GetReadPrimitiveText_(
        ITypeSymbol sourceSymbol,
        IPrimitiveMemberType primitiveMemberType) {
      var primitiveType = primitiveMemberType.PrimitiveType;
      var altFormat = primitiveMemberType.AltFormat;

      var readType = SchemaGeneratorUtil
          .GetPrimitiveLabel(primitiveMemberType.UseAltFormat
                                 ? altFormat.AsPrimitiveType()
                                 : primitiveType);
      var readText = $"{READER}.Read{readType}()";

      var isBoolean = primitiveType == SchemaPrimitiveType.BOOLEAN;
      if (isBoolean) {
        readText += " != 0";
      }

      var castText = "";
      if (primitiveMemberType.UseAltFormat &&
          !isBoolean &&
          primitiveType !=
          altFormat.AsPrimitiveType().GetUnderlyingPrimitiveType()) {
        var castType = primitiveType == SchemaPrimitiveType.ENUM
            ? SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                sourceSymbol,
                primitiveMemberType.TypeSymbol)
            : SchemaGeneratorUtil.GetTypeName(
                primitiveMemberType.PrimitiveType.AsNumberType());
        castText = $"({castType}) ";
      }

      return $"{castText}{readText}";
    }

    private static string GetAssertPrimitiveText_(
        IPrimitiveMemberType primitiveMemberType,
        string accessText) {
      var primitiveType = primitiveMemberType.PrimitiveType;
      var altFormat = primitiveMemberType.AltFormat;

      var assertType = primitiveMemberType.UseAltFormat
          ? altFormat.AsPrimitiveType()
          : primitiveType;
      var assertLabel = SchemaGeneratorUtil.GetPrimitiveLabel(assertType);
      var assertMethod = $"{READER}.Assert{assertLabel}";

      bool needToCast;
      if (primitiveType == SchemaPrimitiveType.BOOLEAN) {
        accessText += " ? 1 : 0";
        needToCast = !assertType.AsIntegerType().CanAcceptAnInt32();
        if (needToCast) {
          accessText = $"({accessText})";
        }
      } else {
        needToCast = primitiveMemberType.UseAltFormat &&
                     primitiveType !=
                     altFormat.AsPrimitiveType().GetUnderlyingPrimitiveType();
      }

      var castText = "";
      if (needToCast) {
        var castType =
            SchemaGeneratorUtil.GetTypeName(assertType.AsNumberType());
        castText = $"({castType}) ";
      }

      return $"{assertMethod}({castText}{accessText})";
    }
  }
}