using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.binary.dependencies;
using schema.binary.parser;
using schema.binary.steps;
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

      if (container.DependsOnSchemaUtil()) {
        dependencies.Add("schema.util");
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
          HandleSteps_(sw, container.ToBinarySteps());
          sw.ExitBlock();

          // TODO: Handle fancier cases here

          // type
          sw.ExitBlock();
        });

    var generatedCode = sb.ToString();
    return generatedCode;
  }

    private static void HandleSteps_(
      ISourceWriter sw,
      IEnumerable<IBinaryStep> steps) {
    foreach (var step in steps) {
      HandleStep_(sw, step);
    }
  }

  private static void HandleStep_(
      ISourceWriter sw,
      IBinaryStep step) {
    switch (step) {
      case AlignStartBinaryStep alignStartBinaryStep: {
        var alignStart = alignStartBinaryStep.Attribute;
        var valueName = alignStart.Method switch {
            AlignSourceType.CONST        => $"{alignStart.ConstAlign}",
            AlignSourceType.OTHER_MEMBER => $"{alignStart.OtherMember.Name}"
        };
        sw.WriteLine($"{WRITER}.Align({valueName});");
        break;
      }
      case AlignEndBinaryStep alignEndBinaryStep: {
        var alignEnd = alignEndBinaryStep.Attribute;
        var valueName = alignEnd.Method switch {
            AlignSourceType.CONST => $"{alignEnd.ConstAlign}",
        };
        sw.WriteLine($"{WRITER}.Align({valueName});");
        break;
      }
      case AtPositionBinarySteps atPositionBinarySteps: {
        // (We ignore the at position logic at write time.)
        HandleSteps_(
            sw,
            atPositionBinarySteps.ValidOffsetChildren);
        break;
      }
      case IfBooleanBinarySteps ifBooleanBinarySteps: {
        var ifBoolean = ifBooleanBinarySteps.Attribute;
        var member = ifBooleanBinarySteps.Member;
        var shouldSkipWhenNull = member.MemberType.TypeInfo.IsNullable &&
                                 member.MemberType is not IContainerMemberType;
        if (ifBoolean.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE) {
          sw.WriteLine(
              $"{GetWritePrimitiveText_(SchemaPrimitiveType.BOOLEAN, ifBoolean.ImmediateBooleanType.AsNumberType(), $"this.{member.Name} != null")};");
        }

        if (shouldSkipWhenNull) {
          sw.EnterBlock($"if (this.{member.Name} != null)");
        }

        HandleSteps_(sw, ifBooleanBinarySteps.TrueChildren);

        if (shouldSkipWhenNull) {
          sw.ExitBlock();
        }

        break;
      }
      case LocalSpaceBinaryStep localSpaceBinaryStep: {
        sw.WriteLine($"{WRITER}.PushLocalSpace();");
        HandleSteps_(sw, localSpaceBinaryStep.Children);
        sw.WriteLine($"{WRITER}.PopLocalSpace();");
        break;
      }
      case ContainerEndiannessBinaryStep containerEndiannessBinaryStep: {
        var endianness = containerEndiannessBinaryStep.Endianness;
        sw.WriteLine(
            $"{WRITER}.PushContainerEndianness({SchemaGeneratorUtil.GetEndiannessName(endianness)});");
        HandleSteps_(sw,
                     containerEndiannessBinaryStep.Children);
        sw.WriteLine($"{WRITER}.PopEndianness();");
        break;
      }
      case MemberEndiannessBinaryStep memberEndiannessBinaryStep: {
        var endianness = memberEndiannessBinaryStep.Endianness;
        sw.WriteLine(
            $"{WRITER}.PushMemberEndianness({SchemaGeneratorUtil.GetEndiannessName(endianness)});");
        HandleSteps_(sw,
                     memberEndiannessBinaryStep.Children);
        sw.WriteLine($"{WRITER}.PopEndianness();");
        break;
      }
      case MemberBinaryStep memberBinaryStep: {
        if (memberBinaryStep.Member is ISchemaValueMember valueMmeber) {
          WriteValueMember_(sw, valueMmeber);
        }
        break;
      }
      case ClearMemberBinaryStep: {
        // (Skip.)
        break;
      }
      default: throw new ArgumentOutOfRangeException(nameof(step));
    }
  }

  private static void WriteValueMember_(
      ISourceWriter sw,
      ISchemaValueMember member) {
    if (member.IsSkipped) {
      return;
    }

    if (member.IsPosition) {
      return;
    }

    var memberType = member.MemberType;
    if (memberType is IGenericMemberType genericMemberType) {
      memberType = genericMemberType.ConstraintType;
    }

    var shouldSkipWhenNull = member.MemberType.TypeInfo.IsNullable &&
                             member.MemberType is not IContainerMemberType &&
                             member.IfBoolean == null;
    if (shouldSkipWhenNull) {
      sw.EnterBlock($"if (this.{member.Name} != null)");
    }

    switch (memberType) {
      case IPrimitiveMemberType: {
        WritePrimitive_(sw, member);
        break;
      }
      case IStringType: {
        WriteString_(sw, member);
        break;
      }
      case IContainerMemberType containerMemberType: {
        WriteContainer_(
            sw,
            containerMemberType,
            member);
        break;
      }
      case IKnownStructMemberType knownStructMemberType: {
        WriteKnownStruct_(
            sw,
            knownStructMemberType,
            member);
        break;
      }
      case ISequenceMemberType: {
        WriteArray_(sw, member);
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

  private static void HandleMemberTracking_(
      ISourceWriter sw,
      ISchemaValueMember member,
      Action handler) {
    var shouldTrackStartAndEnd = member.TrackStartAndEnd;
    if (shouldTrackStartAndEnd) {
      sw.WriteLine($"{WRITER}.MarkStartOfMember(\"{member.Name}\");");
    }

    handler();

    if (shouldTrackStartAndEnd) {
      sw.WriteLine($"{WRITER}.MarkEndOfMember();");
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

    HandleMemberTracking_(
        sw,
        member,
        () => {
          if (member.MemberType is IFloatMemberType {
                  FixedPointAttribute: not null
              } floatMemberType) {
            var target
                = floatMemberType.PrimitiveType is SchemaPrimitiveType.SINGLE
                    ? "Single"
                    : "Double";
            var fixedPointAttribute = floatMemberType.FixedPointAttribute;
            sw.WriteLine(
                $"{GetWritePrimitiveText_(
                    SchemaPrimitiveType.UINT32,
                    fixedPointAttribute.IntegerType.AsNumberType(),
                    $"BitLogic.Convert{target}ToFixedPoint(this.{member.Name}, {fixedPointAttribute.SignBits}, {fixedPointAttribute.IntegerBits}, {fixedPointAttribute.FractionBits})")};");
            return;
          }

          var writeType = useAltFormat
              ? altFormat.AsPrimitiveType()
              : primitiveType;
          var writeTypeLabel =
              SchemaGeneratorUtil.GetPrimitiveLabel(writeType);

          var integerMemberType = primitiveMemberType as IIntegerMemberType;
          var isNotDelayed = integerMemberType
              is null
                 or {
                     SizeOfStream: false,
                     AccessChainToSizeOf: null,
                     PointerToAttribute: null
                 };
          if (isNotDelayed) {
            var accessText = $"this.{member.Name}";
            if (member.MemberType.TypeInfo.IsNullable) {
              accessText = $"{accessText}.Value";
            }

            var lengthOfStringMembers
                = integerMemberType?.LengthOfStringMembers;
            var isLengthOfString = lengthOfStringMembers is { Length: > 0 };
            if (isLengthOfString) {
              accessText = $"{lengthOfStringMembers[0].Name}.Length";
              if (lengthOfStringMembers.Length > 1) {
                sw.WriteLine(
                    $"Asserts.AllEqual({string.Join(", ", lengthOfStringMembers.Select(member => $"{member.Name}.Length"))});");
              }
            }

            var lengthOfSequenceMembers
                = integerMemberType?.LengthOfSequenceMembers;
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

            var accessChainToSizeOf = integerMemberType?.AccessChainToSizeOf;
            var pointerToAttribute = integerMemberType?.PointerToAttribute;
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
    HandleMemberTracking_(
        sw,
        member,
        () => {
          var stringType =
              Asserts.CastNonnull(member.MemberType as IStringType);

          var encodingType = "";
          if (stringType.EncodingType != StringEncodingType.ASCII) {
            encodingType = $"StringEncodingType.{stringType.EncodingType}";
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

    HandleMemberTracking_(
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
    HandleMemberTracking_(
        sw,
        member,
        () => {
          var accessText = $"this.{member.Name}";
          if (knownStructMemberType.TypeInfo.IsNullable) {
            accessText += ".Value";
          }

          var knownStructName
              = SchemaGeneratorUtil.GetKnownStructName(
                  knownStructMemberType.KnownStruct);
          sw.WriteLine($"{WRITER}.Write{knownStructName}({accessText});");
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

    WriteIntoArray_(sw, member);
  }

  private static void WriteIntoArray_(ISourceWriter sw,
                                      ISchemaValueMember member) {
    HandleMemberTracking_(
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

          if (TryToWriteManyIntoArrayAtOnce_(sw, sequenceMemberType, member)) {
            return;
          }

          if (elementType is IPrimitiveMemberType primitiveElementType) {
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

  private static bool TryToWriteManyIntoArrayAtOnce_(
      ISourceWriter sw,
      ISequenceMemberType sequenceMemberType,
      ISchemaValueMember member) {
    if (!SchemaGeneratorUtil.TryToGetSequenceAsSpan(sequenceMemberType,
                                                    member,
                                                    out var spanText)) {
      return false;
    }

    var elementType = sequenceMemberType.ElementType;
    if (elementType is IGenericMemberType
        genericElementType) {
      elementType =
          genericElementType.ConstraintType;
    }

    if (!SchemaGeneratorUtil.TryToGetLabelForMethodWithoutCast(
            elementType!,
            out var label)) {
      return false;
    }

    sw.WriteLine($"{WRITER}.Write{label}s({spanText});");
    return true;
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