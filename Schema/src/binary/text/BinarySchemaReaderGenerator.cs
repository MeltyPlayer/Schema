using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.binary.dependencies;
using schema.util;

namespace schema.binary.text {
  public class BinarySchemaReaderGenerator {
    public string Generate(IBinarySchemaStructure structure) {
      var typeSymbol = structure.TypeSymbol;

      var typeNamespace = SymbolTypeUtil.MergeContainingNamespaces(typeSymbol);

      var declaringTypes =
          SymbolTypeUtil.GetDeclaringTypesDownward(typeSymbol);

      var sb = new StringBuilder();
      var cbsb = new CurlyBracketTextWriter(new StringWriter(sb));

      {
        var dependencies = new List<string> { "System", "System.IO" };
        if (structure.DependsOnSequenceImports()) {
          dependencies.Add("schema.util.sequences");
        }

        if (structure.DependsOnSchemaAttributes()) {
          dependencies.Add("schema.binary.attributes");
        }

        if (structure.DependsOnCollectionsImports()) {
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
        cbsb.EnterBlock(SymbolTypeUtil.GetQualifiersAndNameFor(declaringType));
      }

      cbsb.EnterBlock(SymbolTypeUtil.GetQualifiersAndNameFor(typeSymbol));

      cbsb.EnterBlock("public void Read(IEndianBinaryReader er)");
      {
        var hasLocalPositions = structure.LocalPositions;
        if (hasLocalPositions) {
          cbsb.WriteLine("er.PushLocalSpace();");
        }

        var hasEndianness = structure.Endianness != null;
        if (hasEndianness) {
          cbsb.WriteLine(
              $"er.PushStructureEndianness({SchemaGeneratorUtil.GetEndiannessName(structure.Endianness.Value)});");
        }

        foreach (var member in structure.Members) {
          if (member is ISchemaValueMember valueMember) {
            BinarySchemaReaderGenerator.ReadValueMember_(
                cbsb,
                typeSymbol,
                valueMember);
          } else if (member is ISchemaMethodMember) {
            cbsb.WriteLine($"this.{member.Name}(er);");
          }
        }

        if (hasEndianness) {
          cbsb.WriteLine("er.PopEndianness();");
        }

        if (hasLocalPositions) {
          cbsb.WriteLine("er.PopLocalSpace();");
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
        ITypeSymbol sourceSymbol,
        ISchemaValueMember member) {
      if (member.IsPosition) {
        if (member.MemberType.IsReadOnly) {
          cbsb.WriteLine($"er.AssertPosition(this.{member.Name});");
        } else {
          cbsb.WriteLine($"this.{member.Name} = er.Position;");
        }

        return;
      }

      if (member.IsIgnored) {
        return;
      }

      // TODO: How to handle both offset & if boolean together?

      var offset = member.Offset;
      if (offset != null) {
        cbsb.EnterBlock()
            .WriteLine("var tempLocation = er.Position;")
            .WriteLine(
                $"er.Position = this.{offset.OffsetName.Name};");
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
          cbsb.WriteLine($"var b = er.Read{booleanPrimitiveLabel}() != 0;")
              .EnterBlock("if (b)");
        } else {
          cbsb.EnterBlock($"if (this.{ifBoolean.OtherMember.Name})");
        }

        if (member.MemberType is not IPrimitiveMemberType &&
            member.MemberType is not ISequenceMemberType {
                SequenceTypeInfo.SequenceType: SequenceType
                    .MUTABLE_ARRAY
            }) {
          cbsb.WriteLine(
              $"this.{member.Name} = new {sourceSymbol.GetQualifiedNameFromCurrentSymbol(member.MemberType.TypeSymbol)}();");
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
        case IStructureMemberType structureMemberType: {
          BinarySchemaReaderGenerator.ReadStructure_(cbsb,
            structureMemberType,
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
        cbsb.WriteLine("er.Position = tempLocation;")
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
          AlignSourceType.CONST        => $"{align.ConstAlign}",
          AlignSourceType.OTHER_MEMBER => $"{align.OtherMember.Name}"
      };
      cbsb.WriteLine($"er.Align({valueName});");
    }

    private static void HandleMemberEndianness_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member,
        Action handler) {
      var hasEndianness = member.Endianness != null;
      if (hasEndianness) {
        cbsb.WriteLine(
            $"er.PushMemberEndianness({SchemaGeneratorUtil.GetEndiannessName(member.Endianness.Value)});");
      }

      BinarySchemaReaderGenerator.Align_(cbsb, member);

      handler();

      if (hasEndianness) {
        cbsb.WriteLine("er.PopEndianness();");
      }
    }

    private static void ReadPrimitive_(
        ICurlyBracketTextWriter cbsb,
        ITypeSymbol sourceSymbol,
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
                  sourceSymbol,
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
                    $"er.AssertStringNT({encodingTypeWithComma}this.{member.Name});");
              } else {
                cbsb.WriteLine(
                    $"er.AssertString({encodingTypeWithComma}this.{member.Name});");
              }

              return;
            }

            if (stringType.LengthSourceType ==
                StringLengthSourceType.NULL_TERMINATED) {
              cbsb.WriteLine(
                  $"this.{member.Name} = er.ReadStringNT({encodingType});");
              return;
            }

            var readMethod = stringType.IsNullTerminated
                ? "ReadStringNT"
                : "ReadString";

            if (stringType.LengthSourceType == StringLengthSourceType.CONST) {
              cbsb.WriteLine(
                  $"this.{member.Name} = er.{readMethod}({encodingTypeWithComma}{stringType.ConstLength});");
              return;
            }

            if (stringType.LengthSourceType ==
                StringLengthSourceType.IMMEDIATE_VALUE) {
              var readType =
                  SchemaGeneratorUtil.GetIntLabel(
                      stringType.ImmediateLengthType);
              cbsb.EnterBlock()
                  .WriteLine($"var l = er.Read{readType}();")
                  .WriteLine(
                      $"this.{member.Name} = er.{readMethod}({encodingTypeWithComma}l);")
                  .ExitBlock();
              return;
            }

            if (stringType.LengthSourceType ==
                StringLengthSourceType.OTHER_MEMBER) {
              cbsb.WriteLine(
                  $"this.{member.Name} = er.{readMethod}({encodingTypeWithComma}{stringType.LengthMember.Name});");
              return;
            }

            // TODO: Handle more cases
            throw new NotImplementedException();
          });
    }

    private static void ReadStructure_(
        ICurlyBracketTextWriter cbsb,
        IStructureMemberType structureMemberType,
        ISchemaValueMember member) {
      // TODO: Do value types need to be handled differently?
      var memberName = member.Name;
      if (structureMemberType.IsChild) {
        cbsb.WriteLine($"this.{memberName}.Parent = this;");
      }

      HandleMemberEndianness_(
          cbsb,
          member,
          () => {
            if (!structureMemberType.IsStruct) {
              cbsb.WriteLine($"this.{memberName}.Read(er);");
            } else {
              cbsb.EnterBlock()
                  .WriteLine($"var value = this.{memberName};")
                  .WriteLine("value.Read(er);")
                  .WriteLine($"this.{memberName} = value;")
                  .ExitBlock();
            }
          });
    }

    private static void ReadGeneric_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member) {
      // TODO: Handle generic types beyond just IBinaryConvertible

      var structureMemberType =
          Asserts.CastNonnull(member.MemberType as IStructureMemberType);

      // TODO: Do value types need to be handled differently?
      var memberName = member.Name;
      if (structureMemberType.IsChild) {
        cbsb.WriteLine($"this.{memberName}.Parent = this;");
      }

      HandleMemberEndianness_(cbsb,
                              member,
                              () => {
                                cbsb.WriteLine($"this.{memberName}.Read(er);");
                              });
    }

    private static void ReadArray_(
        ICurlyBracketTextWriter cbsb,
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
              SizeUtil.TryGetSizeOfType(arrayType.ElementType, out var size)) {
            var remainingLengthAccessor = "er.Length - er.Position";
            var readCountAccessor = size == 1
                ? remainingLengthAccessor
                : $"({remainingLengthAccessor}) / {size}";

            // Primitives that don't need to be cast are the easiest to read.
            if (!primitiveElementType.UseAltFormat) {
              var label =
                  SchemaGeneratorUtil.GetPrimitiveLabel(
                      primitiveElementType.PrimitiveType);
              cbsb.WriteLine(
                  $"{memberAccessor} = er.Read{label}s({readCountAccessor});");
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
          cbsb.WriteLine($"var {target} = new List<{qualifiedElementName}>();");
        }

        {
          cbsb.EnterBlock("while (!er.Eof)");
          {
            var elementType = arrayType.ElementType;
            if (elementType is IGenericMemberType genericElementType) {
              elementType = genericElementType.ConstraintType;
            }

            if (elementType is IPrimitiveMemberType primitiveElementType) {
              cbsb.WriteLine(
                  $"{target}.Add({GetReadPrimitiveText_(sourceSymbol, primitiveElementType)});");
            } else if
                (elementType is IStructureMemberType structureElementType) {
              cbsb.WriteLine($"var e = new {qualifiedElementName}();");

              if (structureElementType.IsChild) {
                cbsb.WriteLine("e.Parent = this;");
              }

              cbsb.WriteLine("e.Read(er);");
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
            SequenceLengthSourceType.CONST_LENGTH => $"{arrayType.ConstLength}",
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
              .WriteLine($"var {lengthName} = er.Read{readType}();");
        }

        var inPlace =
            arrayType.SequenceTypeInfo.SequenceType == SequenceType.MUTABLE_LIST
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
        ITypeSymbol sourceSymbol,
        ISchemaValueMember member) {
      HandleMemberEndianness_(
          cbsb,
          member,
          () => {
            var sequenceType =
                Asserts.CastNonnull(
                    member.MemberType as
                        ISequenceMemberType);

            if (sequenceType.SequenceTypeInfo.SequenceType is SequenceType
                    .MUTABLE_SEQUENCE or SequenceType.READ_ONLY_SEQUENCE) {
              cbsb.WriteLine($"this.{member.Name}.Read(er);");
              return;
            }

            var elementType = sequenceType.ElementType;
            if (elementType is IGenericMemberType
                genericElementType) {
              elementType =
                  genericElementType.ConstraintType;
            }

            if (elementType is IPrimitiveMemberType
                primitiveElementType) {
              // Primitives that don't need to be cast are the easiest to read.
              if (!primitiveElementType.UseAltFormat) {
                var label =
                    SchemaGeneratorUtil.GetPrimitiveLabel(
                        primitiveElementType.PrimitiveType);
                if (!primitiveElementType.IsReadOnly) {
                  cbsb.WriteLine(
                      $"er.Read{label}s(this.{member.Name});");
                } else {
                  cbsb.WriteLine(
                      $"er.Assert{label}s(this.{member.Name});");
                }

                return;
              }

              // Primitives that *do* need to be cast have to be read individually.
              if (!primitiveElementType.IsReadOnly) {
                var arrayLengthName =
                    sequenceType.SequenceTypeInfo.LengthName;

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
                            sourceSymbol,
                            primitiveElementType,
                            "e")};")
                    .ExitBlock();
              }

              return;
            }

            if (elementType is IStructureMemberType
                structureElementType) {
              if (!structureElementType.IsStruct) {
                cbsb.EnterBlock(
                    $"foreach (var e in this.{member.Name})");

                if (structureElementType.IsChild) {
                  cbsb.WriteLine("e.Parent = this;");
                }

                cbsb.WriteLine("e.Read(er);");
                cbsb.ExitBlock();
              } else {
                var arrayLengthName =
                    sequenceType.SequenceTypeInfo.LengthName;
                cbsb.EnterBlock(
                    $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)");
                cbsb.WriteLine(
                    $"var e = this.{member.Name}[i];");

                if (structureElementType.IsChild) {
                  cbsb.WriteLine("e.Parent = this;");
                }

                cbsb.WriteLine("e.Read(er);");
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
        ITypeSymbol sourceSymbol,
        IPrimitiveMemberType primitiveMemberType) {
      var primitiveType = primitiveMemberType.PrimitiveType;
      var altFormat = primitiveMemberType.AltFormat;

      var readType = SchemaGeneratorUtil
          .GetPrimitiveLabel(primitiveMemberType.UseAltFormat
                                 ? altFormat.AsPrimitiveType()
                                 : primitiveType);
      var readText = $"er.Read{readType}()";

      var isBoolean = primitiveType == SchemaPrimitiveType.BOOLEAN;
      if (isBoolean) {
        readText += " != 0";
      }

      var castText = "";
      if (primitiveMemberType.UseAltFormat && !isBoolean &&
          primitiveType != SchemaPrimitiveTypesUtil.GetUnderlyingPrimitiveType(
              altFormat.AsPrimitiveType())) {
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
        ITypeSymbol sourceSymbol,
        IPrimitiveMemberType primitiveMemberType,
        string accessText) {
      var primitiveType = primitiveMemberType.PrimitiveType;
      var altFormat = primitiveMemberType.AltFormat;

      var assertType = primitiveMemberType.UseAltFormat
          ? altFormat.AsPrimitiveType()
          : primitiveType;
      var assertLabel = SchemaGeneratorUtil.GetPrimitiveLabel(assertType);
      var assertMethod = $"er.Assert{assertLabel}";

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
                     SchemaPrimitiveTypesUtil.GetUnderlyingPrimitiveType(
                         altFormat.AsPrimitiveType());

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