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
using schema.util.symbols;

namespace schema.binary.text {
  public class BinarySchemaWriterGenerator {
    public string Generate(IBinarySchemaContainer container) {
      var typeSymbol = container.TypeSymbol;

      var typeNamespace = typeSymbol.GetFullyQualifiedNamespace();

      var declaringTypes =
          SymbolTypeUtil.GetDeclaringTypesDownward(typeSymbol);

      var sb = new StringBuilder();
      var cbsb = new CurlyBracketTextWriter(new StringWriter(sb));

      {
        var dependencies = new List<string> { "System", "schema.binary" };

        if (container.DependsOnSchemaAttributes()) {
          dependencies.Add("schema.binary.attributes");
        }

        if (container.DependsOnSchemaUtil()) {
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
        var hasLocalPositions = container.LocalPositions;
        if (hasLocalPositions) {
          cbsb.WriteLine("ew.PushLocalSpace();");
        }

        var hasEndianness = container.Endianness != null;
        if (hasEndianness) {
          cbsb.WriteLine(
              $"ew.PushContainerEndianness({SchemaGeneratorUtil.GetEndiannessName(container.Endianness.Value)});");
        }

        foreach (var member in container.Members.OfType<ISchemaValueMember>()) {
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
          cbsb.WriteLine(
                  $"{GetWritePrimitiveText_(SchemaPrimitiveType.BOOLEAN, ifBoolean.ImmediateBooleanType.AsNumberType(), $"this.{member.Name} != null")};")
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
        case IContainerMemberType: {
          BinarySchemaWriterGenerator.WriteContainer_(cbsb, member);
          break;
        }
        case ISequenceMemberType: {
          BinarySchemaWriterGenerator.WriteArray_(cbsb, member);
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
        cbsb.WriteLine("ew.MarkEndOfMember();");
      }

      if (hasEndianness) {
        cbsb.WriteLine("ew.PopEndianness();");
      }
    }

    private static void WritePrimitive_(
        ICurlyBracketTextWriter cbsb,
        ISchemaValueMember member) {
      var primitiveMemberType = member.MemberType as IPrimitiveMemberType;
      if (primitiveMemberType == null) {
        Asserts.Fail();
        return;
      }

      var useAltFormat = primitiveMemberType.UseAltFormat;
      var altFormat = primitiveMemberType.AltFormat;
      var primitiveType = primitiveMemberType.PrimitiveType;

      HandleMemberEndiannessAndTracking_(
          cbsb,
          member,
          () => {
            var writeType = useAltFormat
                ? altFormat.AsPrimitiveType()
                : primitiveType;
            var writeTypeLabel =
                SchemaGeneratorUtil.GetPrimitiveLabel(writeType);

            var isNotDelayed =
                !primitiveMemberType.SizeOfStream
                && primitiveMemberType.AccessChainToSizeOf == null
                && primitiveMemberType.AccessChainToPointer == null;
            if (isNotDelayed) {
              var accessText = $"this.{member.Name}";
              if (member.MemberType.TypeInfo.IsNullable) {
                accessText = $"{accessText}.Value";
              }

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

              if ((isLengthOfString || isLengthOfSequence) &&
                  !primitiveType.AsIntegerType().CanAcceptAnInt32()) {
                cbsb.WriteLine(
                    $"{GetWritePrimitiveText_(
                        SchemaPrimitiveType.INT32,
                        primitiveType.AsNumberType(),
                        accessText)};");
              } else {
                cbsb.WriteLine(
                    $"{GetWritePrimitiveText_(primitiveMemberType, accessText)};");
              }
            } else {
              var needToCast = primitiveType != SchemaPrimitiveType.INT64;

              var castText = "";
              if (needToCast) {
                var castType =
                    SchemaGeneratorUtil.GetTypeName(
                        primitiveType.AsNumberType());
                castText =
                    $".ContinueWith(task => ({castType}) task.Result)";
              }

              string accessText;
              var typeChain = primitiveMemberType.AccessChainToSizeOf ??
                              primitiveMemberType.AccessChainToPointer;
              if (typeChain != null) {
                accessText = primitiveMemberType.AccessChainToSizeOf != null
                    ? $"ew.GetSizeOfMemberRelativeToScope(\"{typeChain.Path}\")"
                    : $"ew.GetPointerToMemberRelativeToScope(\"{typeChain.Path}\")";
              } else {
                accessText =
                    "ew.GetAbsoluteLength()";
              }

              cbsb.WriteLine(
                  $"ew.Write{writeTypeLabel}Delayed({accessText}{castText});");
            }
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

    private static void WriteContainer_(
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
        ISchemaValueMember member) {
      var sequenceMemberType =
          Asserts.CastNonnull(member.MemberType as ISequenceMemberType);
      if (sequenceMemberType.LengthSourceType !=
          SequenceLengthSourceType.READ_ONLY) {
        var isImmediate =
            sequenceMemberType.LengthSourceType ==
            SequenceLengthSourceType.IMMEDIATE_VALUE;

        if (isImmediate) {
          var arrayLengthName = sequenceMemberType.SequenceTypeInfo.LengthName;
          var arrayLengthAccessor = $"this.{member.Name}.{arrayLengthName}";

          cbsb.WriteLine(
              $"{GetWritePrimitiveText_(SchemaPrimitiveType.INT32, sequenceMemberType.ImmediateLengthType.AsNumberType(), arrayLengthAccessor)};");
        }
      }

      BinarySchemaWriterGenerator.WriteIntoArray_(cbsb, member);
    }

    private static void WriteIntoArray_(ICurlyBracketTextWriter cbsb,
                                        ISchemaValueMember member) {
      HandleMemberEndiannessAndTracking_(
          cbsb,
          member,
          () => {
            var sequenceMemberType =
                Asserts.CastNonnull(member.MemberType as ISequenceMemberType);
            var sequenceTypeInfo = sequenceMemberType.SequenceTypeInfo;
            var sequenceType = sequenceTypeInfo.SequenceType;

            if (sequenceType.IsISequence()) {
              cbsb.WriteLine($"this.{member.Name}.Write(ew);");
              return;
            }

            var elementType =
                sequenceMemberType.ElementType;
            if (elementType is IGenericMemberType
                genericElementType) {
              elementType = genericElementType
                  .ConstraintType;
            }

            if (elementType is IPrimitiveMemberType primitiveElementType) {
              // Primitives that don't need to be cast are the easiest to write.
              if (!primitiveElementType.UseAltFormat &&
                  sequenceType.IsArray()) {
                var label =
                    SchemaGeneratorUtil.GetPrimitiveLabel(
                        primitiveElementType.PrimitiveType);
                cbsb.WriteLine(
                    $"ew.Write{label}s(this.{member.Name});");
                return;
              }

              // Primitives that *do* need to be cast have to be written individually.
              var arrayLengthName = sequenceTypeInfo.LengthName;
              cbsb.EnterBlock(
                      $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)")
                  .WriteLine(
                      $"{GetWritePrimitiveText_(primitiveElementType, $"this.{member.Name}[i]")};")
                  .ExitBlock();
              return;
            }

            if (elementType is IContainerMemberType) {
              cbsb.EnterBlock(
                      $"foreach (var e in this.{member.Name})")
                  .WriteLine("e.Write(ew);")
                  .ExitBlock();
              return;
            }

            // Anything that makes it down here probably isn't meant to be written.
            throw new NotImplementedException();
          });
    }

    private static string GetWritePrimitiveText_(
        IPrimitiveMemberType primitiveMemberType,
        string accessText)
      => GetWritePrimitiveText_(
          primitiveMemberType.UseAltFormat,
          primitiveMemberType.PrimitiveType,
          primitiveMemberType.AltFormat.AsPrimitiveType(),
          accessText);

    private static string GetWritePrimitiveText_(
        bool useAltFormat,
        SchemaPrimitiveType primitiveType,
        SchemaPrimitiveType altFormat,
        string accessText) {
      var writeType = useAltFormat ? altFormat : primitiveType;
      var writeMethod =
          $"ew.Write{SchemaGeneratorUtil.GetPrimitiveLabel(writeType)}";

      bool needToCast = false;
      if (primitiveType == SchemaPrimitiveType.BOOLEAN) {
        accessText = $"{accessText} ? 1 : 0";
        needToCast = !altFormat.AsIntegerType().CanAcceptAnInt32();
        if (needToCast) {
          accessText = $"({accessText})";
        }
      } else if (useAltFormat) {
        needToCast = primitiveType != altFormat.GetUnderlyingPrimitiveType();
      }

      var castText = "";
      if (needToCast) {
        var castType =
            SchemaGeneratorUtil.GetTypeName(writeType.AsNumberType());
        castText = $"({castType}) ";
      }


      return $"{writeMethod}({castText}{accessText})";
    }


    private static string GetWritePrimitiveText_(
        SchemaPrimitiveType srcType,
        SchemaNumberType dstType,
        string accessText) {
      var dstPrimitiveType = dstType.AsPrimitiveType();
      var writeType = SchemaGeneratorUtil.GetPrimitiveLabel(dstPrimitiveType);
      var writeMethod = $"ew.Write{writeType}";

      bool needToCast;
      if (srcType == SchemaPrimitiveType.BOOLEAN) {
        accessText = $"{accessText} ? 1 : 0";
        needToCast = !dstPrimitiveType.AsIntegerType().CanAcceptAnInt32();
        if (needToCast) {
          accessText = $"({accessText})";
        }
      } else {
        needToCast = dstType.AsPrimitiveType() !=
                     srcType.GetUnderlyingPrimitiveType();
      }

      var castText = "";
      if (needToCast) {
        var castType = SchemaGeneratorUtil.GetTypeName(dstType);
        castText = $"({castType}) ";
      }

      return $"{writeMethod}({castText}{accessText})";
    }
  }
}