using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.binary.dependencies;
using schema.binary.parser;
using schema.util.asserts;
using schema.util.symbols;
using schema.util.text;


namespace schema.binary.text;

public class BinarySchemaWriterGenerator {
  public const string WRITER = "bw";

  public string Generate(IBinarySchemaContainer container) {
    var typeSymbol = container.TypeSymbol;

    var sb = new StringBuilder();
    using var sw = new SourceWriter(new StringWriter(sb));

    {
      var dependencies = new List<string> { "System", "schema.binary" };

      if (container.DependsOnSchemaAttributes()) {
        dependencies.Add("schema.binary.attributes");
      }

      if (container.DependsOnSchemaUtilAsserts()) {
        dependencies.Add("schema.util.asserts");
      }

      if (container.DependsOnSystemThreadingTasks()) {
        dependencies.Add("System.Threading.Tasks");
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

          sw.EnterBlock($"public void Write(IBinaryWriter {WRITER})");
          {
            var hasLocalPositions = container.LocalPositions;
            if (hasLocalPositions) {
              sw.WriteLine($"{WRITER}.PushLocalSpace();");
            }

            var hasEndianness = container.Endianness != null;
            if (hasEndianness) {
              sw.WriteLine(
                  $"{WRITER}.PushContainerEndianness({SchemaGeneratorUtil.GetEndiannessName(container.Endianness.Value)});");
            }

            foreach (var member in container.Members
                                            .OfType<ISchemaValueMember>()) {
              BinarySchemaWriterGenerator.WriteValueMember_(
                  sw,
                  typeSymbol,
                  member);
            }

            if (hasEndianness) {
              sw.WriteLine($"{WRITER}.PopEndianness();");
            }

            if (hasLocalPositions) {
              sw.WriteLine($"{WRITER}.PopLocalSpace();");
            }

            AlignEndOfContainer_(sw, container);
          }
          sw.ExitBlock();

          // TODO: Handle fancier cases here

          // type
          sw.ExitBlock();
        });

    var generatedCode = sb.ToString();
    return generatedCode;
  }

  private static void WriteValueMember_(
      ISourceWriter sw,
      ITypeSymbol sourceSymbol,
      ISchemaValueMember member) {
    if (member.IsSkipped) {
      return;
    }

    if (member.IsPosition) {
      return;
    }

    var shouldSkipWhenNull = member.MemberType.TypeInfo.IsNullable &&
                             member.MemberType is not IContainerMemberType;
    var ifBoolean = member.IfBoolean;
    if (ifBoolean?.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE) {
      sw.WriteLine(
          $"{GetWritePrimitiveText_(SchemaPrimitiveType.BOOLEAN, ifBoolean.ImmediateBooleanType.AsNumberType(), $"this.{member.Name} != null")};");
    }

    if (shouldSkipWhenNull) {
      sw.EnterBlock($"if (this.{member.Name} != null)");
    }

    var memberType = member.MemberType;
    if (memberType is IGenericMemberType genericMemberType) {
      memberType = genericMemberType.ConstraintType;
    }

    switch (memberType) {
      case IPrimitiveMemberType: {
        BinarySchemaWriterGenerator.WritePrimitive_(sw, member);
        break;
      }
      case IStringType: {
        BinarySchemaWriterGenerator.WriteString_(sw, member);
        break;
      }
      case IContainerMemberType containerMemberType: {
        BinarySchemaWriterGenerator.WriteContainer_(
            sw,
            containerMemberType,
            member);
        break;
      }
      case IKnownStructMemberType knownStructMemberType: {
        BinarySchemaWriterGenerator.WriteKnownStruct_(
            sw,
            knownStructMemberType,
            member);
        break;
      }
      case ISequenceMemberType: {
        BinarySchemaWriterGenerator.WriteArray_(sw, member);
        break;
      }
      default:
        // Anything that makes it down here probably isn't meant to be read.
        throw new NotImplementedException();
    }

    if (shouldSkipWhenNull) {
      sw.ExitBlock();
    }
  }

  private static void AlignStartOfMember_(
      ISourceWriter sw,
      ISchemaValueMember member) {
    var alignStart = member.AlignStart;
    if (alignStart == null) {
      return;
    }

    var valueName = alignStart.Method switch {
        AlignSourceType.CONST        => $"{alignStart.ConstAlign}",
        AlignSourceType.OTHER_MEMBER => $"{alignStart.OtherMember.Name}"
    };
    sw.WriteLine($"{WRITER}.Align({valueName});");
  }

  private static void AlignEndOfContainer_(
      ISourceWriter sw,
      IBinarySchemaContainer container) {
    var alignEnd = container.AlignEnd;
    if (alignEnd == null) {
      return;
    }

    var valueName = alignEnd.Method switch {
        AlignSourceType.CONST => $"{alignEnd.ConstAlign}",
    };
    sw.WriteLine($"{WRITER}.Align({valueName});");
  }

  private static void HandleMemberEndiannessAndTracking_(
      ISourceWriter sw,
      ISchemaValueMember member,
      Action handler) {
    BinarySchemaWriterGenerator.AlignStartOfMember_(sw, member);

    var hasEndianness = member.Endianness != null;
    if (hasEndianness) {
      sw.WriteLine(
          $"{WRITER}.PushMemberEndianness({SchemaGeneratorUtil.GetEndiannessName(member.Endianness.Value)});");
    }

    var shouldTrackStartAndEnd = member.TrackStartAndEnd;
    if (shouldTrackStartAndEnd) {
      sw.WriteLine($"{WRITER}.MarkStartOfMember(\"{member.Name}\");");
    }

    handler();

    if (shouldTrackStartAndEnd) {
      sw.WriteLine($"{WRITER}.MarkEndOfMember();");
    }

    if (hasEndianness) {
      sw.WriteLine($"{WRITER}.PopEndianness();");
    }
  }

  private static void WritePrimitive_(
      ISourceWriter sw,
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
        sw,
        member,
        () => {
          var writeType = useAltFormat
              ? altFormat.AsPrimitiveType()
              : primitiveType;
          var writeTypeLabel =
              SchemaGeneratorUtil.GetPrimitiveLabel(writeType);

          var isNotDelayed =
              !primitiveMemberType.SizeOfStream &&
              primitiveMemberType.AccessChainToSizeOf == null &&
              primitiveMemberType.PointerToAttribute == null;
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
                sw.WriteLine(
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
                sw.WriteLine(
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
              sw.WriteLine(
                  $"{GetWritePrimitiveText_(
                      SchemaPrimitiveType.INT32,
                      primitiveType.AsNumberType(),
                      accessText)};");
            } else {
              sw.WriteLine(
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

            var accessChainToSizeOf = primitiveMemberType.AccessChainToSizeOf;
            var pointerToAttribute = primitiveMemberType.PointerToAttribute;
            string accessText;
            if (accessChainToSizeOf != null) {
              accessText =
                  $"{WRITER}.GetSizeOfMemberRelativeToScope(\"{accessChainToSizeOf.Path}\")";
            } else if (pointerToAttribute != null) {
              var accessChain = pointerToAttribute.AccessChainToOtherMember;
              accessText =
                  $"{WRITER}.GetPointerToMemberRelativeToScope(\"{accessChain.Path}\")";

              var nullValue = pointerToAttribute.NullValue;
              if (nullValue != null) {
                accessText =
                    $"(this.{accessChain.RawPath} == null ? Task.FromResult({nullValue.Value}L) : {accessText})";
              }
            } else {
              accessText = $"{WRITER}.GetAbsoluteLength()";
            }

            sw.WriteLine(
                $"{WRITER}.Write{writeTypeLabel}Delayed({accessText}{castText});");
          }
        });
  }

  private static void WriteString_(
      ISourceWriter sw,
      ISchemaValueMember member) {
    HandleMemberEndiannessAndTracking_(
        sw,
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
            sw.WriteLine(
                $"{WRITER}.WriteStringNT({encodingTypeWithComma}this.{member.Name});");
          } else if (stringType.LengthSourceType ==
                     StringLengthSourceType.CONST) {
            sw.WriteLine(
                $"{WRITER}.WriteStringWithExactLength({encodingTypeWithComma}this.{member.Name}, {stringType.ConstLength});");
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
            sw.WriteLine(
                  $"{WRITER}.Write{writeType}({castText}{accessText});")
              .WriteLine(
                  $"{WRITER}.WriteString({encodingTypeWithComma}this.{member.Name});");
          } else {
            sw.WriteLine(
                $"{WRITER}.WriteString({encodingTypeWithComma}this.{member.Name});");
          }
        });
  }

  private static void WriteContainer_(
      ISourceWriter sw,
      IContainerMemberType containerMemberType,
      ISchemaValueMember member) {
    var memberName = member.Name;
    if (containerMemberType.IsChild) {
      sw.WriteLine($"this.{memberName}.Parent = this;");
    }

    HandleMemberEndiannessAndTracking_(
        sw,
        member,
        () => {
          if (containerMemberType.TypeInfo.IsNullable) {
            sw.WriteLine($"this.{memberName}?.Write({WRITER});");
          } else {
            sw.WriteLine($"this.{memberName}.Write({WRITER});");
          }
        });
  }

  private static void WriteKnownStruct_(
      ISourceWriter sw,
      IKnownStructMemberType knownStructMemberType,
      ISchemaValueMember member) {
    HandleMemberEndiannessAndTracking_(
        sw,
        member,
        () => {
          var memberName = member.Name;
          var knownStructName
              = SchemaGeneratorUtil.GetKnownStructName(
                  knownStructMemberType.KnownStruct);
          sw.WriteLine($"{WRITER}.Write{knownStructName}(this.{memberName});");
        });
  }

  private static void WriteArray_(
      ISourceWriter sw,
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

        sw.WriteLine(
            $"{GetWritePrimitiveText_(SchemaPrimitiveType.INT32, sequenceMemberType.ImmediateLengthType.AsNumberType(), arrayLengthAccessor)};");
      }
    }

    BinarySchemaWriterGenerator.WriteIntoArray_(sw, member);
  }

  private static void WriteIntoArray_(ISourceWriter sw,
                                      ISchemaValueMember member) {
    HandleMemberEndiannessAndTracking_(
        sw,
        member,
        () => {
          var sequenceMemberType =
              Asserts.CastNonnull(member.MemberType as ISequenceMemberType);
          var sequenceTypeInfo = sequenceMemberType.SequenceTypeInfo;
          var sequenceType = sequenceTypeInfo.SequenceType;

          if (sequenceType.IsISequence()) {
            sw.WriteLine($"this.{member.Name}.Write({WRITER});");
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
              sw.WriteLine(
                  $"{WRITER}.Write{label}s(this.{member.Name});");
              return;
            }

            // Primitives that *do* need to be cast have to be written individually.
            var arrayLengthName = sequenceTypeInfo.LengthName;
            sw.EnterBlock(
                  $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)")
              .WriteLine(
                  $"{GetWritePrimitiveText_(primitiveElementType, $"this.{member.Name}[i]")};")
              .ExitBlock();
            return;
          }

          if (elementType is IContainerMemberType containerElementType) {
            sw.EnterBlock($"foreach (var e in this.{member.Name})");

            if (containerElementType.IsChild) {
              sw.WriteLine("e.Parent = this;");
            }

            sw.WriteLine($"e.Write({WRITER});")
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
        $"{WRITER}.Write{SchemaGeneratorUtil.GetPrimitiveLabel(writeType)}";

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
    var writeMethod = $"{WRITER}.Write{writeType}";

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