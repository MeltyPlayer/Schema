using System;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary.util;


namespace schema.binary.text {
  public class BinarySchemaReaderGenerator {
    public string Generate(IBinarySchemaStructure structure) {
      var typeSymbol = structure.TypeSymbol;

      var typeNamespace = SymbolTypeUtil.MergeContainingNamespaces(typeSymbol);

      var declaringTypes =
          SymbolTypeUtil.GetDeclaringTypesDownward(typeSymbol);

      var sb = new StringBuilder();
      var cbsb = new CurlyBracketTextWriter(new StringWriter(sb));
      cbsb.WriteLine("using System;")
          .WriteLine("using System.Collections.Generic;")
          .WriteLine("using System.IO;");

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
        var hasEndianness = structure.Endianness != null;
        if (hasEndianness) {
          cbsb.WriteLine(
              $"er.PushStructureEndianness({SchemaGeneratorUtil.GetEndiannessName(structure.Endianness.Value)});");
        }

        foreach (var member in structure.Members) {
          BinarySchemaReaderGenerator.ReadMember_(cbsb, typeSymbol, member);
        }

        if (hasEndianness) {
          cbsb.WriteLine("er.PopEndianness();");
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

    private static void ReadMember_(
        ICurlyBracketTextWriter cbsb,
        ITypeSymbol sourceSymbol,
        ISchemaMember member) {
      if (member.IsPosition) {
        if (member.MemberType.IsReadonly) {
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
                $"er.Position = this.{offset.StartIndexName.Name} + this.{offset.OffsetName.Name};");
      }

      var ifBoolean = member.IfBoolean;
      var immediateIfBoolean =
          ifBoolean?.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE;
      if (immediateIfBoolean) {
        cbsb.EnterBlock();
      }

      if (ifBoolean != null) {
        if (ifBoolean.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE) {
          var booleanNumberType =
              SchemaPrimitiveTypesUtil.ConvertIntToNumber(
                  ifBoolean.ImmediateBooleanType);
          var booleanPrimitiveType =
              SchemaPrimitiveTypesUtil.ConvertNumberToPrimitive(
                  booleanNumberType);
          var booleanPrimitiveLabel =
              SchemaGeneratorUtil.GetPrimitiveLabel(booleanPrimitiveType);
          cbsb.WriteLine($"var b = er.Read{booleanPrimitiveLabel}() != 0;")
              .EnterBlock("if (b)");
        } else {
          cbsb.EnterBlock($"if (this.{ifBoolean.BooleanMember.Name})");
        }

        if (member.MemberType is not IPrimitiveMemberType &&
            member.MemberType is not ISequenceMemberType {
                SequenceType: SequenceType.ARRAY
            }) {
          cbsb.WriteLine(
              $"this.{member.Name} = new {SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(sourceSymbol, member.MemberType.TypeSymbol)}();");
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
        ISchemaMember member) {
      var align = member.Align;
      if (align != 0) {
        cbsb.WriteLine($"er.Align({align});");
      }
    }

    private static void HandleMemberEndianness_(
        ICurlyBracketTextWriter cbsb,
        ISchemaMember member,
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
        ISchemaMember member) {
      var primitiveType =
          Asserts.CastNonnull(member.MemberType as IPrimitiveMemberType);

      if (primitiveType.PrimitiveType == SchemaPrimitiveType.BOOLEAN) {
        BinarySchemaReaderGenerator.ReadBoolean_(cbsb, member);
        return;
      }

      HandleMemberEndianness_(cbsb,
                              member,
                              () => {
                                var readType =
                                    SchemaGeneratorUtil.GetPrimitiveLabel(
                                        primitiveType.UseAltFormat
                                            ? SchemaPrimitiveTypesUtil
                                                .ConvertNumberToPrimitive(
                                                    primitiveType.AltFormat)
                                            : primitiveType.PrimitiveType);

                                var needToCast = primitiveType.UseAltFormat &&
                                                 primitiveType.PrimitiveType !=
                                                 SchemaPrimitiveTypesUtil
                                                     .GetUnderlyingPrimitiveType(
                                                         SchemaPrimitiveTypesUtil
                                                             .ConvertNumberToPrimitive(
                                                                 primitiveType
                                                                     .AltFormat));

                                if (!primitiveType.IsReadonly) {
                                  var castText = "";
                                  if (needToCast) {
                                    var castType =
                                        primitiveType.PrimitiveType ==
                                        SchemaPrimitiveType.ENUM
                                            ? SymbolTypeUtil
                                                .GetQualifiedNameFromCurrentSymbol(
                                                    sourceSymbol,
                                                    primitiveType.TypeSymbol)
                                            : primitiveType.TypeSymbol.Name;
                                    castText = $"({castType}) ";
                                  }

                                  cbsb.WriteLine(
                                      $"this.{member.Name} = {castText}er.Read{readType}();");
                                } else {
                                  var castText = "";
                                  if (needToCast) {
                                    var castType =
                                        SchemaGeneratorUtil.GetTypeName(
                                            primitiveType.AltFormat);
                                    castText = $"({castType}) ";
                                  }

                                  cbsb.WriteLine(
                                      $"er.Assert{readType}({castText}this.{member.Name});");
                                }
                              });
    }

    private static void ReadBoolean_(
        ICurlyBracketTextWriter cbsb,
        ISchemaMember member) {
      HandleMemberEndianness_(cbsb,
                              member,
                              () => {
                                var primitiveType =
                                    Asserts.CastNonnull(
                                        member.MemberType as
                                            IPrimitiveMemberType);

                                var readType =
                                    SchemaGeneratorUtil.GetPrimitiveLabel(
                                        SchemaPrimitiveTypesUtil
                                            .ConvertNumberToPrimitive(
                                                primitiveType.AltFormat));

                                if (!primitiveType.IsReadonly) {
                                  cbsb.WriteLine(
                                      $"this.{member.Name} = er.Read{readType}() != 0;");
                                } else {
                                  cbsb.WriteLine(
                                      $"er.Assert{readType}(this.{member.Name} ? 1 : 0);");
                                }
                              });
    }

    private static void ReadString_(
        ICurlyBracketTextWriter cbsb,
        ISchemaMember member) {
      HandleMemberEndianness_(cbsb,
                              member,
                              () => {
                                var stringType =
                                    Asserts.CastNonnull(
                                        member.MemberType as IStringType);

                                if (stringType.IsReadonly) {
                                  if (stringType.LengthSourceType ==
                                      StringLengthSourceType.NULL_TERMINATED) {
                                    cbsb.WriteLine(
                                        $"er.AssertStringNT(this.{member.Name});");
                                  } else {
                                    cbsb.WriteLine(
                                        $"er.AssertString(this.{member.Name});");
                                  }

                                  return;
                                }

                                if (stringType.LengthSourceType ==
                                    StringLengthSourceType.NULL_TERMINATED) {
                                  cbsb.WriteLine(
                                      $"this.{member.Name} = er.ReadStringNT();");
                                  return;
                                }

                                if (stringType.LengthSourceType ==
                                    StringLengthSourceType.CONST) {
                                  cbsb.WriteLine(
                                      $"this.{member.Name} = er.ReadString({stringType.ConstLength});");
                                  return;
                                }

                                if (stringType.LengthSourceType ==
                                    StringLengthSourceType.IMMEDIATE_VALUE) {
                                  var readType =
                                      SchemaGeneratorUtil.GetIntLabel(
                                          stringType.ImmediateLengthType);
                                  cbsb.EnterBlock()
                                      .WriteLine(
                                          $"var l = er.Read{readType}();")
                                      .WriteLine(
                                          $"this.{member.Name} = er.ReadString(l);")
                                      .ExitBlock();
                                  return;
                                }

                                // TODO: Handle more cases
                                throw new NotImplementedException();
                              });
    }

    private static void ReadStructure_(
        ICurlyBracketTextWriter cbsb,
        IStructureMemberType structureMemberType,
        ISchemaMember member) {
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

    private static void ReadGeneric_(
        ICurlyBracketTextWriter cbsb,
        ISchemaMember member) {
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
        ISchemaMember member) {
      var arrayType =
          Asserts.CastNonnull(member.MemberType as ISequenceMemberType);
      if (arrayType.LengthSourceType ==
          SequenceLengthSourceType.UNTIL_END_OF_STREAM) {
        var qualifiedElementName =
            SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                sourceSymbol,
                arrayType.ElementType.TypeSymbol);

        var memberAccessor = $"this.{member.Name}";

        var isArray = arrayType.SequenceType == SequenceType.ARRAY;
        {
          if (isArray &&
              arrayType.ElementType is IPrimitiveMemberType
                  primitiveElementType &&
              SizeUtil.TryGetSizeOfType(arrayType.ElementType, out var size)) {
            var remainingLengthAccessor = "er.Length - er.Position";
            var readCountAccessor = size == 1
                ? remainingLengthAccessor
                : $"{remainingLengthAccessor} / {size}";

            // Primitives that don't need to be cast are the easiest to read.
            if (!primitiveElementType.UseAltFormat) {
              var label =
                  SchemaGeneratorUtil.GetPrimitiveLabel(
                      primitiveElementType.PrimitiveType);
              cbsb.WriteLine(
                  $"{memberAccessor} = er.Read{label}s({readCountAccessor});");
            } else {
              // Primitives that *do* need to be cast have to be read individually.
              var readType = SchemaGeneratorUtil.GetPrimitiveLabel(
                  SchemaPrimitiveTypesUtil.ConvertNumberToPrimitive(
                      primitiveElementType.AltFormat));
              var castType =
                  primitiveElementType.PrimitiveType ==
                  SchemaPrimitiveType.ENUM
                      ? SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                          sourceSymbol,
                          primitiveElementType.TypeSymbol)
                      : primitiveElementType.TypeSymbol.Name;
              cbsb.WriteLine(
                      $"{memberAccessor} = new {qualifiedElementName}[{readCountAccessor}];")
                  .EnterBlock(
                      $"for (var i = 0; i < {memberAccessor}.Length; ++i)")
                  .WriteLine(
                      $"{memberAccessor}[i] = ({castType}) er.Read{readType}();")
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
              // Primitives that don't need to be cast are the easiest to read.
              if (!primitiveElementType.UseAltFormat) {
                var label =
                    SchemaGeneratorUtil.GetPrimitiveLabel(
                        primitiveElementType.PrimitiveType);
                cbsb.WriteLine($"{target}.Add(er.Read{label}());");
              } else {
                // Primitives that *do* need to be cast have to be read individually.
                var readType = SchemaGeneratorUtil.GetPrimitiveLabel(
                    SchemaPrimitiveTypesUtil.ConvertNumberToPrimitive(
                        primitiveElementType.AltFormat));
                var castType =
                    primitiveElementType.PrimitiveType ==
                    SchemaPrimitiveType.ENUM
                        ? SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                            sourceSymbol,
                            primitiveElementType.TypeSymbol)
                        : primitiveElementType.TypeSymbol.Name;
                cbsb.WriteLine(
                    $"{target}.Add(({castType}) er.Read{readType}();");
              }
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
                 SequenceLengthSourceType.READONLY) {
        var isImmediate =
            arrayType.LengthSourceType ==
            SequenceLengthSourceType.IMMEDIATE_VALUE;

        var lengthName = arrayType.LengthSourceType switch {
            SequenceLengthSourceType.IMMEDIATE_VALUE => "c",
            SequenceLengthSourceType.OTHER_MEMBER =>
                $"this.{arrayType.LengthMember!.Name}",
            SequenceLengthSourceType.CONST_LENGTH => $"{arrayType.ConstLength}",
        };

        if (isImmediate) {
          var readType = SchemaGeneratorUtil.GetIntLabel(
              arrayType.ImmediateLengthType);
          cbsb.EnterBlock()
              .WriteLine($"var {lengthName} = er.Read{readType}();");
        }

        if (arrayType.LengthSourceType is SequenceLengthSourceType
                .IMMEDIATE_VALUE or SequenceLengthSourceType.OTHER_MEMBER) {
          cbsb.EnterBlock($"if ({lengthName} < 0)")
              .WriteLine(
                  $"throw new Exception(\"Expected length to be nonnegative!\");")
              .ExitBlock();
        }

        var qualifiedElementName =
            SymbolTypeUtil.GetQualifiedNameFromCurrentSymbol(
                sourceSymbol,
                arrayType.ElementType.TypeSymbol);
        var hasReferenceElements =
            arrayType.ElementType is IStructureMemberType {
                IsReferenceType: true
            };

        // TODO: Handle readonly lists, can't be expanded like this!
        if (arrayType.SequenceType == SequenceType.LIST) {
          cbsb.EnterBlock($"while (this.{member.Name}.Count < {lengthName})");
          if (hasReferenceElements) {
            cbsb.WriteLine(
                $"this.{member.Name}.Add(new {qualifiedElementName}());");
          } else {
            cbsb.WriteLine($"this.{member.Name}.Add(default);");
          }

          cbsb.ExitBlock();

          cbsb.EnterBlock(
                  $"while (this.{member.Name}.Count > {lengthName})")
              .WriteLine($"this.{member.Name}.RemoveAt(0);")
              .ExitBlock();
        } else {
          cbsb.WriteLine(
              $"this.{member.Name} = new {qualifiedElementName}[{lengthName}];");

          if (hasReferenceElements) {
            cbsb.EnterBlock($"for (var i = 0; i < {lengthName}; ++i)")
                .WriteLine(
                    $"this.{member.Name}[i] = new {qualifiedElementName}();")
                .ExitBlock();
          }
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
        ISchemaMember member) {
      HandleMemberEndianness_(cbsb,
                              member,
                              () => {
                                var arrayType =
                                    Asserts.CastNonnull(
                                        member.MemberType as
                                            ISequenceMemberType);

                                var elementType = arrayType.ElementType;
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
                                    if (!primitiveElementType.IsReadonly) {
                                      cbsb.WriteLine(
                                          $"er.Read{label}s(this.{member.Name});");
                                    } else {
                                      cbsb.WriteLine(
                                          $"er.Assert{label}s(this.{member.Name});");
                                    }

                                    return;
                                  }

                                  // Primitives that *do* need to be cast have to be read individually.
                                  var readType = SchemaGeneratorUtil
                                      .GetPrimitiveLabel(
                                          SchemaPrimitiveTypesUtil
                                              .ConvertNumberToPrimitive(
                                                  primitiveElementType
                                                      .AltFormat));
                                  if (!primitiveElementType.IsReadonly) {
                                    var arrayLengthName =
                                        arrayType.SequenceType ==
                                        SequenceType.ARRAY
                                            ? "Length"
                                            : "Count";
                                    var castType =
                                        primitiveElementType.PrimitiveType ==
                                        SchemaPrimitiveType.ENUM
                                            ? SymbolTypeUtil
                                                .GetQualifiedNameFromCurrentSymbol(
                                                    sourceSymbol,
                                                    primitiveElementType
                                                        .TypeSymbol)
                                            : primitiveElementType.TypeSymbol
                                                .Name;
                                    cbsb.EnterBlock(
                                            $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)")
                                        .WriteLine(
                                            $"this.{member.Name}[i] = ({castType}) er.Read{readType}();")
                                        .ExitBlock();
                                  } else {
                                    var castType =
                                        SchemaGeneratorUtil.GetTypeName(
                                            primitiveElementType.AltFormat);
                                    cbsb.EnterBlock(
                                            $"foreach (var e in this.{member.Name})")
                                        .WriteLine(
                                            $"er.Assert{readType}(({castType}) e);")
                                        .ExitBlock();
                                  }

                                  return;
                                }

                                if (elementType is IStructureMemberType
                                    structureElementType) {
                                  cbsb.EnterBlock(
                                      $"foreach (var e in this.{member.Name})");

                                  if (structureElementType.IsChild) {
                                    cbsb.WriteLine("e.Parent = this;");
                                  }

                                  cbsb.WriteLine("e.Read(er);");
                                  cbsb.ExitBlock();
                                  return;
                                }

// Anything that makes it down here probably isn't meant to be read.
                                throw new NotImplementedException();
                              });
    }
  }
}