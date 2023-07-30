using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.binary.dependencies;
using schema.binary.parser;
using schema.util;

namespace schema.binary.text {
  public class BinarySchemaWriterGenerator {
    public string Generate(IBinarySchemaStructure structure) {
      var typeSymbol = structure.TypeSymbol;

      var typeNamespace = SymbolTypeUtil.MergeContainingNamespaces(typeSymbol);

      var declaringTypes =
          SymbolTypeUtil.GetDeclaringTypesDownward(typeSymbol);

      var sb = new StringBuilder();
      var cbsb = new CurlyBracketTextWriter(new StringWriter(sb));

      {
        var dependencies = new List<string> { "System", "System.IO" };

        if (structure.DependsOnSchemaAttributes()) {
          dependencies.Add("schema.binary.attributes");
        }

        if (structure.DependsOnSchemaUtil()) {
          dependencies.Add("schema.util");
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

      cbsb.EnterBlock("public void Write(ISubEndianBinaryWriter ew)");
      {
        var hasLocalPositions = structure.LocalPositions;
        if (hasLocalPositions) {
          cbsb.WriteLine("ew.PushLocalSpace();");
        }

        var hasEndianness = structure.Endianness != null;
        if (hasEndianness) {
          cbsb.WriteLine(
              $"ew.PushStructureEndianness({SchemaGeneratorUtil.GetEndiannessName(structure.Endianness.Value)});");
        }

        foreach (var member in structure.Members.OfType<ISchemaValueMember>()) {
          BinarySchemaWriterGenerator.WriteValueMember_(
              cbsb,
              typeSymbol,
              member);
        }

        if (hasEndianness) {
          cbsb.WriteLine("ew.PopEndianness();");
        }

        if (hasLocalPositions) {
          cbsb.WriteLine("ew.PopLocalSpace();");
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

    private static void WriteValueMember_(
        ICurlyBracketTextWriter cbsb,
        ITypeSymbol sourceSymbol,
        ISchemaValueMember member) {
      if (member.IsIgnored) {
        return;
      }

      if (member.IsPosition) {
        return;
      }

      var ifBoolean = member.IfBoolean;
      if (ifBoolean != null) {
        if (ifBoolean.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE) {
          var booleanNumberType = ifBoolean.ImmediateBooleanType.AsNumberType();
          var booleanPrimitiveType = booleanNumberType.AsPrimitiveType();
          var booleanNumberLabel =
              SchemaGeneratorUtil.GetTypeName(booleanNumberType);
          var booleanPrimitiveLabel =
              SchemaGeneratorUtil.GetPrimitiveLabel(booleanPrimitiveType);
          cbsb.WriteLine(
                  $"ew.Write{booleanPrimitiveLabel}(({booleanNumberLabel}) (this.{member.Name} != null ? 1 : 0));")
              .EnterBlock($"if (this.{member.Name} != null)");
        } else {
          cbsb.EnterBlock($"if (this.{ifBoolean.OtherMember.Name})");
        }
      }

      var memberType = member.MemberType;
      if (memberType is IGenericMemberType genericMemberType) {
        memberType = genericMemberType.ConstraintType;
      }

      switch (memberType) {
        case IPrimitiveMemberType: {
          BinarySchemaWriterGenerator.WritePrimitive_(cbsb, member);
          break;
        }
        case IStringType: {
          BinarySchemaWriterGenerator.WriteString_(cbsb, member);
          break;
        }
        case IStructureMemberType structureMemberType: {
          BinarySchemaWriterGenerator.WriteStructure_(cbsb, member);
          break;
        }
        case ISequenceMemberType: {
          BinarySchemaWriterGenerator.WriteArray_(cbsb, sourceSymbol, member);
          break;
        }
        default:
          // Anything that makes it down here probably isn't meant to be read.
          throw new NotImplementedException();
      }

      if (ifBoolean != null) {
        cbsb.ExitBlock();
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
      cbsb.WriteLine($"ew.Align({valueName});");
    }

    private static void HandleMemberEndiannessAndTracking_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member,
        Action handler) {
      BinarySchemaWriterGenerator.Align_(cbsb, member);

      var hasEndianness = member.Endianness != null;
      if (hasEndianness) {
        cbsb.WriteLine(
            $"ew.PushMemberEndianness({SchemaGeneratorUtil.GetEndiannessName(member.Endianness.Value)});");
      }

      var shouldTrackStartAndEnd = member.TrackStartAndEnd;
      if (shouldTrackStartAndEnd) {
        cbsb.WriteLine($"ew.MarkStartOfMember(\"{member.Name}\");");
      }

      handler();

      if (shouldTrackStartAndEnd) {
        cbsb.WriteLine($"ew.MarkEndOfMember();");
      }

      if (hasEndianness) {
        cbsb.WriteLine("ew.PopEndianness();");
      }
    }

    private static void WritePrimitive_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member) {
      var primitiveType =
          Asserts.CastNonnull(member.MemberType as IPrimitiveMemberType);

      if (primitiveType.PrimitiveType == SchemaPrimitiveType.BOOLEAN) {
        BinarySchemaWriterGenerator.WriteBoolean_(cbsb, member);
        return;
      }

      HandleMemberEndiannessAndTracking_(
          cbsb,
          member,
          () => {
            var writeType = SchemaGeneratorUtil
                .GetPrimitiveLabel(
                    primitiveType.UseAltFormat
                        ? primitiveType.AltFormat.AsPrimitiveType()
                        : primitiveType.PrimitiveType);

            var isNotDelayed =
                !primitiveType.SizeOfStream
                && primitiveType.AccessChainToSizeOf == null
                && primitiveType.AccessChainToPointer == null;
            if (isNotDelayed) {
              var needToCast =
                  primitiveType.UseAltFormat &&
                  primitiveType.PrimitiveType !=
                  SchemaPrimitiveTypesUtil
                      .GetUnderlyingPrimitiveType(
                          primitiveType.AltFormat.AsPrimitiveType());

              var castText = "";
              if (needToCast) {
                var castType =
                    SchemaGeneratorUtil.GetTypeName(primitiveType.AltFormat);
                castText = $"({castType}) ";
              }

              var accessText = $"this.{member.Name}";
              if (member.MemberType.TypeInfo.IsNullable) {
                accessText = $"{accessText}.Value";
              }

              if (member.MemberType is IPrimitiveMemberType
                  primitiveMemberType) {
                var lengthOfStringMembers =
                    primitiveMemberType.LengthOfStringMembers;
                var isLengthOfString = lengthOfStringMembers is { Length: > 0 };
                if (isLengthOfString) {
                  accessText = $"{lengthOfStringMembers[0].Name}.Length";
                  if (lengthOfStringMembers.Length > 1) {
                    cbsb.WriteLine(
                        $"Asserts.AllEqual({string.Join(", ", lengthOfStringMembers.Select(member => $"{member.Name}.Length"))});");
                  }
                }

                var lengthOfSequenceMembers =
                    primitiveMemberType.LengthOfSequenceMembers;
                var isLengthOfSequence = lengthOfSequenceMembers is
                    { Length: > 0 };
                if (isLengthOfSequence) {
                  var first = lengthOfSequenceMembers[0];
                  var firstLengthName =
                      (first.MemberTypeInfo as ISequenceTypeInfo).LengthName;
                  accessText = $"{first.Name}.{firstLengthName}";
                  if (lengthOfSequenceMembers.Length > 1) {
                    cbsb.WriteLine(
                        $"Asserts.AllEqual({string.Join(", ", lengthOfSequenceMembers.Select(
                            member => {
                              var lengthName =
                                  (member.MemberTypeInfo as ISequenceTypeInfo).LengthName;
                              return $"{member.Name}.{lengthName}";
                            }))});");
                  }
                }

                if (isLengthOfSequence) { }

                if ((isLengthOfString || isLengthOfSequence) &&
                    primitiveType.PrimitiveType != SchemaPrimitiveType.INT32) {
                  var castType =
                      SchemaGeneratorUtil.GetTypeName(
                          primitiveType.PrimitiveType.AsNumberType());
                  castText = $"({castType}) ";
                }
              }

              cbsb.WriteLine(
                  $"ew.Write{writeType}({castText}{accessText});");
            } else {
              var needToCast =
                  primitiveType.PrimitiveType !=
                  SchemaPrimitiveType.INT64;

              var castText = "";
              if (needToCast) {
                var castType =
                    SchemaGeneratorUtil.GetTypeName(
                        primitiveType.PrimitiveType.AsNumberType());
                castText =
                    $".ContinueWith(task => ({castType}) task.Result)";
              }

              string accessText;
              var typeChain =
                  primitiveType
                      .AccessChainToSizeOf ??
                  primitiveType
                      .AccessChainToPointer;
              if (typeChain != null) {
                accessText =
                    primitiveType
                        .AccessChainToSizeOf !=
                    null
                        ? $"ew.GetSizeOfMemberRelativeToScope(\"{typeChain.Path}\")"
                        : $"ew.GetPointerToMemberRelativeToScope(\"{typeChain.Path}\")";
              } else {
                accessText =
                    "ew.GetAbsoluteLength()";
              }

              cbsb.WriteLine(
                  $"ew.Write{writeType}Delayed({accessText}{castText});");
            }
          });
    }

    private static void WriteBoolean_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member) {
      HandleMemberEndiannessAndTracking_(
          cbsb,
          member,
          () => {
            var primitiveType =
                Asserts.CastNonnull(member.MemberType as IPrimitiveMemberType);

            var writeType = SchemaGeneratorUtil
                .GetPrimitiveLabel(primitiveType.AltFormat.AsPrimitiveType());
            var castType =
                SchemaGeneratorUtil.GetTypeName(primitiveType.AltFormat);

            var accessText = $"this.{member.Name}";
            if (member.MemberType.TypeInfo.IsNullable) {
              accessText = $"{accessText}.Value";
            }

            cbsb.WriteLine(
                $"ew.Write{writeType}(({castType}) ({accessText} ? 1 : 0));");
          });
    }

    private static void WriteString_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member) {
      HandleMemberEndiannessAndTracking_(
          cbsb,
          member,
          () => {
            var stringType =
                Asserts.CastNonnull(member.MemberType as IStringType);

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

            if (stringType.LengthSourceType ==
                StringLengthSourceType.NULL_TERMINATED) {
              cbsb.WriteLine(
                  $"ew.WriteStringNT({encodingTypeWithComma}this.{member.Name});");
            } else if (stringType.LengthSourceType ==
                       StringLengthSourceType.CONST) {
              cbsb.WriteLine(
                  $"ew.WriteStringWithExactLength({encodingTypeWithComma}this.{member.Name}, {stringType.ConstLength});");
            } else if (stringType.LengthSourceType ==
                       StringLengthSourceType.IMMEDIATE_VALUE) {
              var immediateLengthType = stringType.ImmediateLengthType;

              var needToCast = !immediateLengthType.CanAcceptAnInt32();

              var castText = "";
              if (needToCast) {
                var castType = immediateLengthType.GetTypeName();
                castText = $"({castType}) ";
              }

              var accessText = $"this.{member.Name}.Length";

              var writeType = stringType.ImmediateLengthType.GetIntLabel();
              cbsb.WriteLine($"ew.Write{writeType}({castText}{accessText});")
                  .WriteLine(
                      $"ew.WriteString({encodingTypeWithComma}this.{member.Name});");
            } else {
              cbsb.WriteLine(
                  $"ew.WriteString({encodingTypeWithComma}this.{member.Name});");
            }
          });
    }

    private static void WriteStructure_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member) {
      HandleMemberEndiannessAndTracking_(
          cbsb,
          member,
          () => {
            // TODO: Do value types need to be handled differently?
            cbsb.WriteLine($"this.{member.Name}.Write(ew);");
          });
    }

    private static void WriteArray_(
        ICurlyBracketTextWriter cbsb,
        ITypeSymbol sourceSymbol,
        ISchemaValueMember member) {
      var arrayType =
          Asserts.CastNonnull(member.MemberType as ISequenceMemberType);
      if (arrayType.LengthSourceType != SequenceLengthSourceType.READ_ONLY) {
        var isImmediate =
            arrayType.LengthSourceType ==
            SequenceLengthSourceType.IMMEDIATE_VALUE;

        if (isImmediate) {
          var writeType = SchemaGeneratorUtil.GetIntLabel(
              arrayType.ImmediateLengthType);

          var castType =
              SchemaGeneratorUtil.GetTypeName(
                  arrayType.ImmediateLengthType.AsNumberType());

          var arrayLengthName = arrayType.SequenceTypeInfo.LengthName;
          var arrayLengthAccessor = $"this.{member.Name}.{arrayLengthName}";

          cbsb.WriteLine(
              $"ew.Write{writeType}(({castType}) {arrayLengthAccessor});");
        }
      }

      BinarySchemaWriterGenerator.WriteIntoArray_(cbsb, sourceSymbol, member);
    }

    private static void WriteIntoArray_(
        ICurlyBracketTextWriter cbsb,
        ITypeSymbol sourceSymbol,
        ISchemaValueMember member) {
      HandleMemberEndiannessAndTracking_(
          cbsb,
          member,
          () => {
            var sequenceType =
                Asserts.CastNonnull(
                    member.MemberType as
                        ISequenceMemberType);

            if (sequenceType.SequenceTypeInfo.SequenceType is SequenceType
                    .MUTABLE_SEQUENCE or SequenceType.READ_ONLY_SEQUENCE) {
              cbsb.WriteLine($"this.{member.Name}.Write(ew);");
              return;
            }

            var elementType =
                sequenceType.ElementType;
            if (elementType is IGenericMemberType
                genericElementType) {
              elementType = genericElementType
                  .ConstraintType;
            }

            if (elementType is
                IPrimitiveMemberType
                primitiveElementType) {
              // Primitives that don't need to be cast are the easiest to write.
              if (!primitiveElementType
                      .UseAltFormat) {
                var label =
                    SchemaGeneratorUtil
                        .GetPrimitiveLabel(
                            primitiveElementType
                                .PrimitiveType);
                cbsb.WriteLine(
                    $"ew.Write{label}s(this.{member.Name});");
                return;
              }

              // Primitives that *do* need to be cast have to be written individually.
              var writeType =
                  SchemaGeneratorUtil
                      .GetPrimitiveLabel(
                          primitiveElementType.AltFormat.AsPrimitiveType());
              var arrayLengthName =
                  sequenceType.SequenceTypeInfo.LengthName;
              var needToCast =
                  primitiveElementType
                      .UseAltFormat &&
                  primitiveElementType
                      .PrimitiveType !=
                  SchemaPrimitiveTypesUtil
                      .GetUnderlyingPrimitiveType(
                          primitiveElementType.AltFormat.AsPrimitiveType());

              var castText = "";
              if (needToCast) {
                var castType =
                    SchemaGeneratorUtil
                        .GetTypeName(
                            primitiveElementType
                                .AltFormat);
                castText = $"({castType}) ";
              }

              cbsb.EnterBlock(
                      $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)")
                  .WriteLine(
                      $"ew.Write{writeType}({castText}this.{member.Name}[i]);")
                  .ExitBlock();
              return;
            }

            if (elementType is
                IStructureMemberType
                structureElementType) {
              //if (structureElementType.IsReferenceType) {
              cbsb.EnterBlock(
                      $"foreach (var e in this.{member.Name})")
                  .WriteLine("e.Write(ew);")
                  .ExitBlock();
              // TODO: Do value types need to be read like below?
              /*}
              // Value types (mainly structs) have to be pulled out, read, then put
              // back in.
              else {
                var arrayLengthName = arrayType.SequenceType == SequenceType.ARRAY
                                          ? "Length"
                                          : "Count";
                cbsb.EnterBlock(
                        $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)")
                    .WriteLine($"var e = this.{member.Name}[i];")
                    .WriteLine("e.Read(ew);")
                    .WriteLine($"this.{member.Name}[i] = e;")
                    .ExitBlock();
              }*/
              return;
            }

            // Anything that makes it down here probably isn't meant to be read.
            throw new NotImplementedException();
          });
    }
  }
}