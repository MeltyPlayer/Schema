using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using schema.binary.attributes;
using schema.binary.dependencies;
using schema.util.asserts;
using schema.util.symbols;
using schema.util.text;
using schema.util.types;

namespace schema.binary.text {
  public class BinarySchemaReaderGenerator {
    public const string READER = "br";

    public string Generate(IBinarySchemaContainer container) {
      var typeSymbol = container.TypeSymbol;
      var typeV2 = TypeV2.FromSymbol(typeSymbol);

      var typeNamespace = typeSymbol.GetFullyQualifiedNamespace();

      var declaringTypes = typeSymbol.GetDeclaringTypesDownward();

      var sb = new StringBuilder();
      using var cbsb = new CurlyBracketTextWriter(new StringWriter(sb));

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
          cbsb.WriteLine($"using {dependency};");
        }

        cbsb.WriteLine("");
      }

      // TODO: Handle fancier cases here
      if (typeNamespace != null) {
        cbsb.EnterBlock($"namespace {typeNamespace}");
      }

      foreach (var declaringType in declaringTypes) {
        cbsb.EnterBlock(declaringType.GetQualifiersAndNameAndGenericParametersFor());
      }

      cbsb.EnterBlock(typeSymbol.GetQualifiersAndNameAndGenericParametersFor());

      cbsb.EnterBlock($"public void Read(IBinaryReader {READER})");
      {
        var hasLocalPositions = container.LocalPositions;
        if (hasLocalPositions) {
          cbsb.WriteLine($"{READER}.PushLocalSpace();");
        }

        var hasEndianness = container.Endianness != null;
        if (hasEndianness) {
          cbsb.WriteLine(
              $"{READER}.PushContainerEndianness({SchemaGeneratorUtil.GetEndiannessName(container.Endianness.Value)});");
        }

        foreach (var member in container.Members) {
          if (member is ISchemaValueMember valueMember) {
            BinarySchemaReaderGenerator.ReadValueMember_(
                cbsb,
                typeV2,
                valueMember);
          } else if (member is ISchemaMethodMember) {
            cbsb.WriteLine($"this.{member.Name}({READER});");
          }
        }

        if (hasEndianness) {
          cbsb.WriteLine($"{READER}.PopEndianness();");
        }

        if (hasLocalPositions) {
          cbsb.WriteLine($"{READER}.PopLocalSpace();");
        }
      }
      cbsb.ExitBlock();

      // TODO: Handle fancier cases here

      // type
      cbsb.ExitBlock();

      // parent types
      foreach (var declaringType in declaringTypes) {
        cbsb.ExitBlock();
      }

      // namespace
      if (typeNamespace != null) {
        cbsb.ExitBlock();
      }

      var generatedCode = sb.ToString();
      return generatedCode;
    }

    private static void ReadValueMember_(
        ICurlyBracketTextWriter cbsb,
        ITypeV2 sourceSymbol,
        ISchemaValueMember member) {
      if (member.IsPosition) {
        if (member.MemberType.IsReadOnly) {
          cbsb.WriteLine($"{READER}.AssertPosition(this.{member.Name});");
        } else {
          cbsb.WriteLine($"this.{member.Name} = {READER}.Position;");
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
          cbsb.EnterBlock($"if (this.{offset.OffsetName.Name} == {nullValue})")
              .WriteLine($"this.{member.Name} = null;")
              .ExitBlock();

          readBlockPrefix = "else";
        }

        cbsb.EnterBlock(readBlockPrefix)
            .WriteLine($"var tempLocation = {READER}.Position;")
            .WriteLine(
                $"{READER}.Position = this.{offset.OffsetName.Name};");
      }

      var ifBoolean = member.IfBoolean;
      var immediateIfBoolean =
          ifBoolean?.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE;
      if (immediateIfBoolean) {
        cbsb.EnterBlock();
      }

      if (ifBoolean != null) {
        if (ifBoolean.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE) {
          var booleanNumberType = ifBoolean.ImmediateBooleanType.AsNumberType();
          var booleanPrimitiveType = booleanNumberType.AsPrimitiveType();
          var booleanPrimitiveLabel =
              SchemaGeneratorUtil.GetPrimitiveLabel(booleanPrimitiveType);
          cbsb.WriteLine(
                  $"var b = {READER}.Read{booleanPrimitiveLabel}() != 0;")
              .EnterBlock("if (b)");
        } else {
          cbsb.EnterBlock($"if (this.{ifBoolean.OtherMember.Name})");
        }

        if (member.MemberType is not IPrimitiveMemberType &&
            member.MemberType is not IContainerMemberType &&
            member.MemberType is not ISequenceMemberType {
              SequenceTypeInfo.SequenceType: SequenceType
                    .MUTABLE_ARRAY
            }) {
          cbsb.WriteLine(
              $"this.{member.Name} = new {sourceSymbol.GetQualifiedNameFromCurrentSymbol(member.MemberType.TypeV2)}();");
        }
      }

      var memberType = member.MemberType;
      if (memberType is IGenericMemberType genericMemberType) {
        memberType = genericMemberType.ConstraintType;
      }

      switch (memberType) {
        case IPrimitiveMemberType: {
            BinarySchemaReaderGenerator.ReadPrimitive_(
                cbsb,
                sourceSymbol,
                member);
            break;
          }
        case IStringType: {
            BinarySchemaReaderGenerator.ReadString_(cbsb, member);
            break;
          }
        case IContainerMemberType containerMemberType: {
            BinarySchemaReaderGenerator.ReadContainer_(cbsb,
              sourceSymbol,
              containerMemberType,
              member);
            break;
          }
        case ISequenceMemberType: {
            BinarySchemaReaderGenerator.ReadArray_(cbsb, sourceSymbol, member);
            break;
          }
        default: {
            // Anything that makes it down here probably isn't meant to be read.
            throw new NotImplementedException();
          }
      }

      if (ifBoolean != null) {
        cbsb.ExitBlock()
            .EnterBlock("else")
            .WriteLine($"this.{member.Name} = null;")
            .ExitBlock();
        if (immediateIfBoolean) {
          cbsb.ExitBlock();
        }
      }

      if (offset != null) {
        cbsb.WriteLine($"{READER}.Position = tempLocation;")
            .ExitBlock();
      }
    }

    private static void Align_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member) {
      var align = member.Align;
      if (align == null) {
        return;
      }

      var valueName = align.Method switch {
        AlignSourceType.CONST => $"{align.ConstAlign}",
        AlignSourceType.OTHER_MEMBER => $"{align.OtherMember.Name}"
      };
      cbsb.WriteLine($"{READER}.Align({valueName});");
    }

    private static void HandleMemberEndianness_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member,
        Action handler) {
      var hasEndianness = member.Endianness != null;
      if (hasEndianness) {
        cbsb.WriteLine(
            $"{READER}.PushMemberEndianness({SchemaGeneratorUtil.GetEndiannessName(member.Endianness.Value)});");
      }

      BinarySchemaReaderGenerator.Align_(cbsb, member);

      handler();

      if (hasEndianness) {
        cbsb.WriteLine($"{READER}.PopEndianness();");
      }
    }

    private static void ReadPrimitive_(
        ICurlyBracketTextWriter cbsb,
        ITypeV2 sourceSymbol,
        ISchemaValueMember member) {
      var primitiveType =
          Asserts.CastNonnull(member.MemberType as IPrimitiveMemberType);

      HandleMemberEndianness_(
          cbsb,
          member,
          () => {
            if (!primitiveType.IsReadOnly) {
              cbsb.WriteLine(
                  $"this.{member.Name} = {GetReadPrimitiveText_(sourceSymbol, primitiveType)};");
            } else {
              cbsb.WriteLine($"{GetAssertPrimitiveText_(
                  primitiveType,
                  $"this.{member.Name}")};");
            }
          });
    }

    private static void ReadString_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member) {
      HandleMemberEndianness_(
          cbsb,
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
                cbsb.WriteLine(
                    $"{READER}.AssertStringNT({encodingTypeWithComma}this.{member.Name});");
              } else {
                cbsb.WriteLine(
                    $"{READER}.AssertString({encodingTypeWithComma}this.{member.Name});");
              }

              return;
            }

            if (stringType.LengthSourceType ==
                StringLengthSourceType.NULL_TERMINATED) {
              cbsb.WriteLine(
                  $"this.{member.Name} = {READER}.ReadStringNT({encodingType});");
              return;
            }

            var readMethod = stringType.IsNullTerminated
                ? "ReadStringNT"
                : "ReadString";

            if (stringType.LengthSourceType == StringLengthSourceType.CONST) {
              cbsb.WriteLine(
                  $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}{stringType.ConstLength});");
              return;
            }

            if (stringType.LengthSourceType ==
                StringLengthSourceType.IMMEDIATE_VALUE) {
              var readType =
                  SchemaGeneratorUtil.GetIntLabel(
                      stringType.ImmediateLengthType);
              cbsb.EnterBlock()
                  .WriteLine($"var l = {READER}.Read{readType}();")
                  .WriteLine(
                      $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}l);")
                  .ExitBlock();
              return;
            }

            if (stringType.LengthSourceType ==
                StringLengthSourceType.OTHER_MEMBER) {
              cbsb.WriteLine(
                  $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}{stringType.LengthMember.Name});");
              return;
            }

            // TODO: Handle more cases
            throw new NotImplementedException();
          });
    }

    private static void ReadContainer_(
        ICurlyBracketTextWriter cbsb,
        ITypeV2 sourceSymbol,
        IContainerMemberType containerMemberType,
        ISchemaValueMember member) {
      // TODO: Do value types need to be handled differently?
      var memberName = member.Name;
      if (containerMemberType.IsChild) {
        cbsb.WriteLine($"this.{memberName}.Parent = this;");
      }

      HandleMemberEndianness_(
          cbsb,
          member,
          () => {
            var isNullable = containerMemberType.TypeInfo.IsNullable;
            var isStruct = containerMemberType.TypeV2.IsStruct;

            var qualifiedTypeName =
                SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                    sourceSymbol,
                    containerMemberType.TypeV2);

            if (isNullable) {
              cbsb.WriteLine(
                  $"this.{memberName} = {READER}.ReadNew<{qualifiedTypeName}>();");
            } else {
              if (!isStruct) {
                cbsb.WriteLine($"this.{memberName}.Read({READER});");
              } else {
                cbsb.EnterBlock()
                    .WriteLine($"var value = this.{memberName};")
                    .WriteLine($"value.Read({READER});")
                    .WriteLine($"this.{memberName} = value;")
                    .ExitBlock();
              }
            }
          });
    }

    private static void ReadArray_(
        ICurlyBracketTextWriter cbsb,
        ITypeV2 sourceSymbol,
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
                arrayType.ElementType.TypeV2);

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
              cbsb.WriteLine(
                  $"{memberAccessor} = {READER}.Read{label}s({readCountAccessor});");
            } else {
              cbsb.WriteLine(
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

        cbsb.EnterBlock();
        if (!isArray) {
          cbsb.WriteLine($"{memberAccessor}.Clear();");
        }

        var target = isArray ? "temp" : $"this.{member.Name}";

        if (isArray) {
          cbsb.WriteLine(
              $"var {target} = new List<{qualifiedElementName}>();");
        }

        {
          cbsb.EnterBlock($"while (!{READER}.Eof)");
          {
            var elementType = arrayType.ElementType;
            if (elementType is IGenericMemberType genericElementType) {
              elementType = genericElementType.ConstraintType;
            }

            if (elementType is IPrimitiveMemberType primitiveElementType) {
              cbsb.WriteLine(
                  $"{target}.Add({GetReadPrimitiveText_(sourceSymbol, primitiveElementType)});");
            } else if
                (elementType is IContainerMemberType containerElementType) {
              cbsb.WriteLine($"var e = new {qualifiedElementName}();");

              if (containerElementType.IsChild) {
                cbsb.WriteLine("e.Parent = this;");
              }

              cbsb.WriteLine($"e.Read({READER});");
              cbsb.WriteLine($"{target}.Add(e);");
            }
          }
          cbsb.ExitBlock();
        }

        if (isArray) {
          cbsb.WriteLine($"this.{member.Name} = {target}.ToArray();");
        }

        cbsb.ExitBlock();

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
          cbsb.EnterBlock()
              .WriteLine($"var {lengthName} = {READER}.Read{readType}();");
        }

        var inPlace =
            arrayType.SequenceTypeInfo.SequenceType ==
            SequenceType.MUTABLE_LIST
            || arrayType.SequenceTypeInfo is {
              SequenceType: SequenceType.MUTABLE_SEQUENCE,
              IsLengthConst: false
            };
        if (inPlace) {
          cbsb.WriteLine(
              $"SequencesUtil.ResizeSequenceInPlace(this.{member.Name}, {castText}{lengthName});");
        } else {
          cbsb.WriteLine(
              $"this.{member.Name} = SequencesUtil.CloneAndResizeSequence(this.{member.Name}, {castText}{lengthName});");
        }

        if (isImmediate) {
          cbsb.ExitBlock();
        }
      }

      BinarySchemaReaderGenerator.ReadIntoArray_(cbsb, sourceSymbol, member);
    }

    private static void ReadIntoArray_(
        ICurlyBracketTextWriter cbsb,
        ITypeV2 sourceSymbol,
        ISchemaValueMember member) {
      HandleMemberEndianness_(
          cbsb,
          member,
          () => {
            var sequenceMemberType =
                Asserts.CastNonnull(member.MemberType as ISequenceMemberType);
            var sequenceTypeInfo = sequenceMemberType.SequenceTypeInfo;
            var sequenceType = sequenceTypeInfo.SequenceType;

            if (sequenceType.IsISequence()) {
              cbsb.WriteLine($"this.{member.Name}.Read({READER});");
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
                  cbsb.WriteLine(
                      $"{READER}.Read{label}s(this.{member.Name});");
                } else {
                  cbsb.WriteLine(
                      $"{READER}.Assert{label}s(this.{member.Name});");
                }

                return;
              }

              // Primitives that *do* need to be cast have to be read individually.
              if (!primitiveElementType.IsReadOnly) {
                var arrayLengthName = sequenceTypeInfo.LengthName;

                cbsb.EnterBlock(
                        $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)")
                    .WriteLine(
                        $"this.{member.Name}[i] = {GetReadPrimitiveText_(sourceSymbol, primitiveElementType)};")
                    .ExitBlock();
              } else {
                cbsb.EnterBlock(
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
              if (!containerElementType.TypeV2.IsStruct) {
                cbsb.EnterBlock(
                    $"foreach (var e in this.{member.Name})");

                if (containerElementType.IsChild) {
                  cbsb.WriteLine("e.Parent = this;");
                }

                cbsb.WriteLine($"e.Read({READER});");
                cbsb.ExitBlock();
              } else {
                var arrayLengthName = sequenceTypeInfo.LengthName;
                cbsb.EnterBlock(
                    $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)");
                cbsb.WriteLine(
                    $"var e = this.{member.Name}[i];");

                if (containerElementType.IsChild) {
                  cbsb.WriteLine("e.Parent = this;");
                }

                cbsb.WriteLine($"e.Read({READER});");
                cbsb.WriteLine(
                    $"this.{member.Name}[i] = e;");

                cbsb.ExitBlock();
              }

              return;
            }

            // Anything that makes it down here probably isn't meant to be read.
            throw new NotImplementedException();
          });
    }

    private static string GetReadPrimitiveText_(
        ITypeV2 sourceSymbol,
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
      if (primitiveMemberType.UseAltFormat && !isBoolean &&
          primitiveType !=
          altFormat.AsPrimitiveType().GetUnderlyingPrimitiveType()) {
        var castType = primitiveType == SchemaPrimitiveType.ENUM
            ? SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                sourceSymbol,
                primitiveMemberType.TypeV2)
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