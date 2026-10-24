using System;
using System.Collections.Generic;
using System.IO;
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

      if (container.DependsOnSchemaUtil()) {
        dependencies.Add("schema.util");
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
          HandleSteps_(sw, typeSymbol, container.ToBinarySteps(), false);
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
      ITypeSymbol sourceSymbol,
      IEnumerable<IBinaryStep> steps,
      bool inBlock) {
    foreach (var step in steps) {
      HandleStep_(sw, sourceSymbol, step, inBlock);
    }
  }

  private static void HandleStep_(
      ISourceWriter sw,
      ITypeSymbol sourceSymbol,
      IBinaryStep step,
      bool inBlock) {
    switch (step) {
      case AlignStartBinaryStep alignStartBinaryStep: {
        var alignStart = alignStartBinaryStep.Attribute;
        var valueName = alignStart.Method switch {
            AlignSourceType.CONST        => $"{alignStart.ConstAlign}",
            AlignSourceType.OTHER_MEMBER => $"{alignStart.OtherMember.Name}"
        };
        sw.WriteLine($"{READER}.Align({valueName});");
        break;
      }
      case AlignEndBinaryStep alignEndBinaryStep: {
        var alignEnd = alignEndBinaryStep.Attribute;
        var valueName = alignEnd.Method switch {
            AlignSourceType.CONST => $"{alignEnd.ConstAlign}",
        };
        sw.WriteLine($"{READER}.Align({valueName});");
        break;
      }
      case AtPositionBinarySteps atPositionBinarySteps: {
        var offset = atPositionBinarySteps.Offset;
        var nullValue = offset.NullValue;

        var conditions = new List<string>();
        if (nullValue != null) {
          conditions.Add($"this.{offset.OffsetName.Name} != {nullValue}");
        }

        HandleMaybeIfBlock_(
            sw,
            conditions,
            enteredBlock => {
              if (!enteredBlock) {
                sw.EnterBlock();
              }

              sw.WriteLine($"var tempLocation = {READER}.Position;")
                .WriteLine(
                    $"{READER}.Position = this.{offset.OffsetName.Name};");

              HandleSteps_(sw,
                           sourceSymbol,
                           atPositionBinarySteps.ValidOffsetChildren,
                           true);

              sw.WriteLine($"{READER}.Position = tempLocation;");

              if (!enteredBlock) {
                sw.ExitBlock();
              }
            },
            () => {
              HandleSteps_(sw,
                           sourceSymbol,
                           atPositionBinarySteps.InvalidValidOffsetChildren,
                           inBlock);
            });
        break;
      }
      case IfBooleanBinarySteps ifBooleanBinarySteps: {
        var ifBoolean = ifBooleanBinarySteps.Attribute;
        if (ifBoolean.SourceType == IfBooleanSourceType.IMMEDIATE_VALUE) {
          sw.EnterBlock();
          var booleanNumberType = ifBoolean.ImmediateBooleanType.AsNumberType();
          var booleanPrimitiveType = booleanNumberType.AsPrimitiveType();
          var booleanPrimitiveLabel =
              SchemaGeneratorUtil.GetPrimitiveLabel(booleanPrimitiveType);
          sw.WriteLine(
                $"var b = {READER}.Read{booleanPrimitiveLabel}() != 0;");
          HandleMaybeIfBlock_(
              sw,
              ["b"],
              enteredBlock => {
                HandleSteps_(sw, sourceSymbol, ifBooleanBinarySteps.TrueChildren, inBlock || enteredBlock);
              },
              () => {
                HandleSteps_(sw, sourceSymbol, ifBooleanBinarySteps.FalseChildren, true);
              });
          sw.ExitBlock();
        } else {
          HandleMaybeIfBlock_(
              sw,
              [$"this.{ifBoolean.OtherMember.Name}"],
              enteredBlock => {
                HandleSteps_(sw, sourceSymbol, ifBooleanBinarySteps.TrueChildren, inBlock || enteredBlock);
              },
              () => {
                HandleSteps_(sw, sourceSymbol, ifBooleanBinarySteps.FalseChildren, true);
              });
        }

        break;
      }
      case LocalSpaceBinaryStep localSpaceBinaryStep: {
        sw.WriteLine($"{READER}.PushLocalSpace();");
        HandleSteps_(sw, sourceSymbol, localSpaceBinaryStep.Children, inBlock);
        sw.WriteLine($"{READER}.PopLocalSpace();");
        break;
      }
      case ContainerEndiannessBinaryStep containerEndiannessBinaryStep: {
        var endianness = containerEndiannessBinaryStep.Endianness;
        sw.WriteLine(
            $"{READER}.PushContainerEndianness({SchemaGeneratorUtil.GetEndiannessName(endianness)});");
        HandleSteps_(sw,
                     sourceSymbol,
                     containerEndiannessBinaryStep.Children,
                     inBlock);
        sw.WriteLine($"{READER}.PopEndianness();");
        break;
      }
      case MemberEndiannessBinaryStep memberEndiannessBinaryStep: {
        var endianness = memberEndiannessBinaryStep.Endianness;
        sw.WriteLine(
            $"{READER}.PushMemberEndianness({SchemaGeneratorUtil.GetEndiannessName(endianness)});");
        HandleSteps_(sw,
                     sourceSymbol,
                     memberEndiannessBinaryStep.Children,
                     inBlock);
        sw.WriteLine($"{READER}.PopEndianness();");
        break;
      }
      case MemberBinaryStep memberBinaryStep: {
        ReadMember_(sw, sourceSymbol, memberBinaryStep.Member, inBlock);
        break;
      }
      case ClearMemberBinaryStep clearMemberBinaryStep: {
        var member = clearMemberBinaryStep.Member;

        if (member.MemberType.TypeInfo.IsNullable) {
          sw.WriteLine($"this.{member.Name} = null;");
        } else {
          switch (member.MemberType.TypeInfo) {
            case IPrimitiveTypeInfo: {
              sw.WriteLine($"this.{member.Name} = default;");
              break;
            }
            case IStringTypeInfo stringTypeInfo: {
              var defaultValue = stringTypeInfo.IsNullable ? "null" : "\"\"";
              sw.WriteLine($"this.{member.Name} = {defaultValue};");
              break;
            }
            case IContainerTypeInfo: {
              sw.WriteLine($"this.{member.Name} = default;");
              break;
            }
            case ISequenceTypeInfo: {
              ResizeArray_(sw, member, "0");
              break;
            }
          }
        }

        break;
      }
      default: throw new ArgumentOutOfRangeException(nameof(step));
    }
  }

  private static void ReadMember_(
      ISourceWriter sw,
      ITypeSymbol sourceSymbol,
      ISchemaMember member,
      bool inBlock) {
    if (member is ISchemaValueMember valueMember) {
      ReadValueMember_(
          sw,
          sourceSymbol,
          valueMember,
          inBlock);
    } else if (member is ISchemaMethodMember) {
      sw.WriteLine($"this.{member.Name}({READER});");
    }
  }

  private static void ReadValueMember_(
      ISourceWriter sw,
      ITypeSymbol sourceSymbol,
      ISchemaValueMember member,
      bool inBlock) {
    if (member.IsPosition) {
      if (member.MemberType.IsReadOnly) {
        sw.WriteLine($"{READER}.AssertPosition(this.{member.Name});");
      } else {
        sw.WriteLine($"this.{member.Name} = {READER}.Position;");
      }

      return;
    }

    var memberType = member.MemberType;
    if (memberType is IGenericMemberType genericMemberType) {
      memberType = genericMemberType.ConstraintType;
    }

    switch (memberType) {
      case IPrimitiveMemberType: {
        ReadPrimitive_(
            sw,
            sourceSymbol,
            member);
        break;
      }
      case IStringType: {
        ReadString_(sw, member, inBlock);
        break;
      }
      case IContainerMemberType containerMemberType: {
        ReadContainer_(sw,
                       sourceSymbol,
                       containerMemberType,
                       member);
        break;
      }
      case IKnownStructMemberType knownStructMemberType: {
        ReadKnownStruct_(sw,
                         sourceSymbol,
                         knownStructMemberType,
                         member);
        break;
      }
      case ISequenceMemberType: {
        ReadArray_(sw, sourceSymbol, member);
        break;
      }
      default: {
        // Anything that makes it down here probably isn't meant to be read.
        throw new NotImplementedException();
      }
    }
  }

  private static void HandleMaybeIfBlock_(ISourceWriter sw,
                                          IReadOnlyList<string> conditions,
                                          Action<bool> trueHandler,
                                          Action? falseHandler = null) {
    var conditionText
        = conditions.Count > 0 ? string.Join(" && ", conditions) : null;
    var enteredBlock = conditionText != null;

    if (enteredBlock) {
      sw.EnterBlock($"if ({conditionText})");
    }

    trueHandler(enteredBlock);

    if (!enteredBlock) {
      return;
    }

    sw.ExitBlock();

    if (falseHandler == null) {
      return;
    }

    sw.EnterBlock("else");
    falseHandler();
    sw.ExitBlock();
  }

  private static void ReadPrimitive_(
      ISourceWriter sw,
      ITypeSymbol sourceSymbol,
      ISchemaValueMember member) {
    var primitiveType =
        Asserts.CastNonnull(member.MemberType as IPrimitiveMemberType);

    if (!primitiveType.IsReadOnly) {
      sw.WriteLine(
          $"this.{member.Name} = {GetReadOneViaMethodText_(sourceSymbol, primitiveType)};");
    } else {
      sw.WriteLine($"{GetAssertOneViaMethodText_(
          primitiveType,
          $"this.{member.Name}")};");
    }
  }

  private static void ReadString_(
      ISourceWriter sw,
      ISchemaValueMember member,
      bool inBlock) {
    var stringType =
        Asserts.CastNonnull(member.MemberType as IStringType);

    var encodingType = "";
    if (stringType.EncodingType != StringEncodingType.ASCII) {
      encodingType = $"StringEncodingType.{stringType.EncodingType}";
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
      var constLength = stringType.ConstLength;

      string readValue; 
      if (constLength == 0) {
        readValue = "\"\"";
      } else {
        readValue
            = $"{READER}.{readMethod}({encodingTypeWithComma}{constLength})";
      }

      sw.WriteLine(
          $"this.{member.Name} = {readValue};");
      return;
    }

    if (stringType.LengthSourceType ==
        StringLengthSourceType.IMMEDIATE_VALUE) {
      var readType =
          SchemaGeneratorUtil.GetIntLabel(
              stringType.ImmediateLengthType);
      if (!inBlock) {
        sw.EnterBlock();
      }

      sw.WriteLine($"var l = {READER}.Read{readType}();")
        .WriteLine(
            $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}l);");

      if (!inBlock) {
        sw.ExitBlock();
      }

      return;
    }

    if (stringType.LengthSourceType ==
        StringLengthSourceType.OTHER_MEMBER) {
      var lengthName = stringType.LengthMember.Name;
      sw.WriteLine(
          $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}{lengthName});");
      return;
    }

    // TODO: Handle more cases
    throw new NotImplementedException();
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

    var isNullable = containerMemberType.TypeInfo.IsNullable;
    var isStruct = containerMemberType.TypeSymbol.IsStruct();

    var qualifiedTypeName
        = sourceSymbol.GetQualifiedNameFromCurrentSymbol(
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
  }

  private static void ReadKnownStruct_(
      ISourceWriter sw,
      ITypeSymbol sourceSymbol,
      IKnownStructMemberType knownStructMemberType,
      ISchemaValueMember member) {
    sw.WriteLine(
        $"this.{member.Name} = {GetReadOneViaMethodText_(sourceSymbol, knownStructMemberType)};");

    // TODO: Handle assertions
    if (knownStructMemberType.IsReadOnly) {
      throw new NotImplementedException();
    }
  }

  private static void ReadArray_(
      ISourceWriter sw,
      ITypeSymbol sourceSymbol,
      ISchemaValueMember member) {
    var arrayType =
        Asserts.CastNonnull(member.MemberType as ISequenceMemberType);

    var sequenceTypeInfo = arrayType.SequenceTypeInfo;
    var sequenceType = sequenceTypeInfo.SequenceType;

    if (sequenceTypeInfo is { IsNullable: true, SequenceType: not SequenceType.MUTABLE_ARRAY }) {
      sw.WriteLine(
          $"this.{member.Name} = new {sourceSymbol.GetQualifiedNameFromCurrentSymbol(member.MemberType.TypeSymbol.AsNonNullable())}();");
    }

    // TODO: Not valid for sequences (yet?)
    if (arrayType.LengthSourceType ==
        SequenceLengthSourceType.UNTIL_END_OF_STREAM) {
      var qualifiedElementName
          = sourceSymbol.GetQualifiedNameFromCurrentSymbol(
              arrayType.ElementType.TypeSymbol);

      var memberAccessor = $"this.{member.Name}";

      var isArray = sequenceType == SequenceType.MUTABLE_ARRAY;
      {
        if (isArray &&
            SizeUtil.TryGetSizeOfType(arrayType.ElementType,
                                      out var size)) {
          var remainingLengthAccessor =
              $"{READER}.Length - {READER}.Position";

          var sizeLog2 = Math.Log(size, 2);

          string readCountAccessor;
          if (size == 1) {
            readCountAccessor = remainingLengthAccessor;
          } else if (sizeLog2 % 1 == 0) {
            readCountAccessor
                = $"({remainingLengthAccessor}) >> {sizeLog2}";
          } else {
            readCountAccessor = $"({remainingLengthAccessor}) / {size}";
          }

          var elementType = arrayType.ElementType;
          if (SchemaGeneratorUtil.TryToGetLabelForMethodWithoutCast(
                  elementType,
                  out var label)) {
            sw.WriteLine(
                $"{memberAccessor} = {READER}.Read{label}s({readCountAccessor});");
          } else {
            sw.WriteLine(
                  $"{memberAccessor} = new {qualifiedElementName}[{readCountAccessor}];")
              .EnterBlock(
                  $"for (var i = 0; i < {memberAccessor}.Length; ++i)")
              .WriteLine(
                  $"{memberAccessor}[i] = {GetReadOneViaMethodText_(sourceSymbol, elementType)};")
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
                $"{target}.Add({GetReadOneViaMethodText_(sourceSymbol, primitiveElementType)});");
          } else if
              (elementType is IContainerMemberType containerElementType) {
            sw.WriteLine(
                $"var e = new {sourceSymbol.GetQualifiedNameFromCurrentSymbol(
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
    }

    var falseHandler = () => ResizeArray_(sw, member, "0");

    var hasConstLength = arrayType.LengthSourceType ==
                         SequenceLengthSourceType.CONST_LENGTH;
    if (hasConstLength && arrayType.ConstLength == 0) {
      falseHandler();
    } else {
      var isImmediate =
          arrayType.LengthSourceType ==
          SequenceLengthSourceType.IMMEDIATE_VALUE;
      var lengthName = arrayType.LengthSourceType switch {
          SequenceLengthSourceType.IMMEDIATE_VALUE => "c",
          SequenceLengthSourceType.OTHER_MEMBER =>
              $"this.{arrayType.LengthMember!.Name}",
          SequenceLengthSourceType.CONST_LENGTH =>
              $"{arrayType.ConstLength}",
          SequenceLengthSourceType.READ_ONLY =>
              $"this.{member.Name}.{arrayType.SequenceTypeInfo.LengthName}"
      };

      if (arrayType.LengthSourceType !=
          SequenceLengthSourceType.READ_ONLY) {
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

        ResizeArray_(sw, member, $"{castText}{lengthName}");

        if (isImmediate) {
          sw.ExitBlock();
        }
      }

      ReadIntoArray_(
          sw,
          sourceSymbol,
          member);
    }
  }

  private static void ResizeArray_(
      ISourceWriter sw,
      ISchemaValueMember member,
      string lengthText) {
    var sequenceType =
        Asserts.CastNonnull(member.MemberType as ISequenceMemberType);
    var inPlace =
        sequenceType.SequenceTypeInfo.SequenceType ==
        SequenceType.MUTABLE_LIST ||
        sequenceType.SequenceTypeInfo is {
            SequenceType: SequenceType.MUTABLE_SEQUENCE,
            IsLengthConst: false
        };
    sw.WriteLine(
        inPlace
            ? $"SequencesUtil.ResizeSequenceInPlace(this.{member.Name}, {lengthText});"
            : $"this.{member.Name} = SequencesUtil.CloneAndResizeSequence(this.{member.Name}, {lengthText});");
  }

  private static void ReadIntoArray_(
      ISourceWriter sw,
      ITypeSymbol sourceSymbol,
      ISchemaValueMember member) {
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

    if (TryToReadManyIntoArrayViaMethod_(sw,
                                         sequenceMemberType,
                                         member)) {
      return;
    }

    if (elementType is IPrimitiveMemberType primitiveElementType) {
      // Primitives that *do* need to be cast have to be read individually.
      if (!primitiveElementType.IsReadOnly) {
        var arrayLengthName = sequenceTypeInfo.LengthName;

        sw.EnterBlock(
              $"for (var i = 0; i < this.{member.Name}.{arrayLengthName}; ++i)")
          .WriteLine(
              $"this.{member.Name}[i] = {GetReadOneViaMethodText_(sourceSymbol, primitiveElementType)};")
          .ExitBlock();
      } else {
        sw.EnterBlock(
              $"foreach (var e in this.{member.Name})")
          .WriteLine(
              $"{GetAssertOneViaMethodText_(primitiveElementType, "e")};")
          .ExitBlock();
      }

      return;
    }

    if (elementType is IContainerMemberType containerElementType) {
      var isStruct = containerElementType.TypeSymbol.IsStruct();
      var isIndexed = containerElementType.TypeSymbol.IsIndexed();
      var isChild = containerElementType.IsChild;

      if (!isStruct && !isIndexed) {
        sw.EnterBlock($"foreach (var e in this.{member.Name})");

        if (isChild) {
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

        if (isChild) {
          sw.WriteLine("e.Parent = this;");
        }

        if (isIndexed) {
          sw.WriteLine("e.Index = i;");
        }

        sw.WriteLine($"e.Read({READER});");

        if (isStruct) {
          sw.WriteLine($"this.{member.Name}[i] = e;");
        }

        sw.ExitBlock();
      }

      return;
    }

    // Anything that makes it down here probably isn't meant to be read.
    throw new NotImplementedException();
  }

  private static bool TryToReadManyIntoArrayViaMethod_(
      ISourceWriter sw,
      ISequenceMemberType sequenceMemberType,
      ISchemaValueMember member) {
    if (!SchemaGeneratorUtil.TryToGetSequenceAsSpan(sequenceMemberType,
                                                    member,
                                                    out var spanText)) {
      return false;
    }

    var elementType = sequenceMemberType.ElementType;
    if (elementType is IGenericMemberType genericElementType) {
      elementType = genericElementType.ConstraintType;
    }

    if (!SchemaGeneratorUtil.TryToGetLabelForMethodWithoutCast(
            elementType!,
            out var label)) {
      return false;
    }

    sw.WriteLine(
        $"{READER}.{(elementType.IsReadOnly ? "Assert" : "Read")}{label}s({spanText});");
    return true;
  }

  private static string GetReadOneViaMethodText_(
      ITypeSymbol sourceSymbol,
      IMemberType memberType) {
    if (memberType is IFloatMemberType {
            FixedPointAttribute: not null
        } floatMemberType) {
      var target = floatMemberType.PrimitiveType is SchemaPrimitiveType.SINGLE
          ? "Single"
          : "Double";
      var fixedPointAttribute = floatMemberType.FixedPointAttribute;
      var readIntegerText =
          $"{READER}.Read{SchemaGeneratorUtil.GetIntLabel(fixedPointAttribute.IntegerType)}()";
      return
          $"BitLogic.ConvertFixedPointTo{target}({readIntegerText}, {fixedPointAttribute.SignBits}, {fixedPointAttribute.IntegerBits}, {fixedPointAttribute.FractionBits})";
    }

    var readLabel = SchemaGeneratorUtil.GetLabelForMethod(memberType);
    var readText = $"{READER}.Read{readLabel}()";

    var castText = "";
    if (memberType is IPrimitiveMemberType primitiveMemberType) {
      var primitiveType = primitiveMemberType.PrimitiveType;
      var isBoolean = primitiveType == SchemaPrimitiveType.BOOLEAN;
      if (isBoolean) {
        readText += " != 0";
      }

      var altFormat = primitiveMemberType.AltFormat;
      if (primitiveMemberType.UseAltFormat &&
          !isBoolean &&
          primitiveType !=
          altFormat.AsPrimitiveType().GetUnderlyingPrimitiveType()) {
        var castType = primitiveType == SchemaPrimitiveType.ENUM
            ? sourceSymbol.GetQualifiedNameFromCurrentSymbol(
                primitiveMemberType.TypeSymbol)
            : SchemaGeneratorUtil.GetTypeName(
                primitiveMemberType.PrimitiveType.AsNumberType());
        castText = $"({castType}) ";
      }
    }

    return $"{castText}{readText}";
  }

  private static string GetAssertOneViaMethodText_(
      IMemberType memberType,
      string accessText) {
    if (memberType is IFloatMemberType {
            FixedPointAttribute: not null
        } floatMemberType) {
      var target = floatMemberType.PrimitiveType is SchemaPrimitiveType.SINGLE
          ? "Single"
          : "Double";
      var fixedPointAttribute = floatMemberType.FixedPointAttribute;
      var assertIntegerText =
          $"{READER}.Assert{SchemaGeneratorUtil.GetIntLabel(fixedPointAttribute.IntegerType)}";
      return
          $"{assertIntegerText}(BitLogic.Convert{target}ToFixedPoint({accessText}, {fixedPointAttribute.SignBits}, {fixedPointAttribute.IntegerBits}, {fixedPointAttribute.FractionBits}))";
    }

    var assertLabel = SchemaGeneratorUtil.GetLabelForMethod(memberType);
    var assertMethod = $"{READER}.Assert{assertLabel}";

    var castText = "";
    if (memberType is IPrimitiveMemberType primitiveMemberType) {
      var primitiveType = primitiveMemberType.PrimitiveType;
      var altFormat = primitiveMemberType.AltFormat;

      var assertType = primitiveMemberType.UseAltFormat
          ? altFormat.AsPrimitiveType()
          : primitiveType;

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

      if (needToCast) {
        var castType =
            SchemaGeneratorUtil.GetTypeName(assertType.AsNumberType());
        castText = $"({castType}) ";
      }
    }

    return $"{assertMethod}({castText}{accessText})";
  }
}