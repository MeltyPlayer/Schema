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
      case IKnownStructMemberType knownStructMemberType: {
        BinarySchemaReaderGenerator.ReadKnownStruct_(sw,
          sourceSymbol,
          knownStructMemberType,
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
    sw.WriteLine($"{READER}.Align({valueName});");
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
    sw.WriteLine($"{READER}.Align({valueName});");
  }

  private static void HandleMemberEndiannessAndAtPosition_(
      ISourceWriter sw,
      ISchemaValueMember member,
      Action<bool> trueHandler,
      Action? falseHandler = null)
    => HandleMemberEndianness_(
        sw,
        member,
        () => HandleAtPosition_(sw,
                                member,
                                trueHandler,
                                falseHandler));

  private static void HandleMemberEndianness_(
      ISourceWriter sw,
      ISchemaValueMember member,
      Action handler) {
    var hasEndianness = member.Endianness != null;
    if (hasEndianness) {
      sw.WriteLine(
          $"{READER}.PushMemberEndianness({SchemaGeneratorUtil.GetEndiannessName(member.Endianness.Value)});");
    }

    BinarySchemaReaderGenerator.AlignStartOfMember_(sw, member);

    handler();

    if (hasEndianness) {
      sw.WriteLine($"{READER}.PopEndianness();");
    }
  }

  private static void HandleAtPosition_(
      ISourceWriter sw,
      ISchemaValueMember member,
      Action<bool> trueHandler,
      Action? falseHandler = null)
    => HandleAtPosition_(sw, member, null, trueHandler, falseHandler);

  private static void HandleAtPosition_(
      ISourceWriter sw,
      ISchemaValueMember member,
      string? checkText,
      Action<bool> trueHandler,
      Action? falseHandler = null) {
    var offset = member.Offset;
    var nullValue = offset?.NullValue;

    var conditions = new List<string>();
    if (nullValue != null) {
      conditions.Add($"this.{offset.OffsetName.Name} != {nullValue}");
    }

    if (offset != null && checkText != null) {
      conditions.Add(checkText);
    }

    HandleMaybeIfBlock_(
        sw,
        conditions,
        enteredBlock => {
          if (offset != null) {
            if (!enteredBlock) {
              sw.EnterBlock();
            }

            sw.WriteLine($"var tempLocation = {READER}.Position;")
              .WriteLine($"{READER}.Position = this.{offset.OffsetName.Name};");
          }

          trueHandler(enteredBlock || offset != null);

          if (offset != null) {
            sw.WriteLine($"{READER}.Position = tempLocation;");

            if (!enteredBlock) {
              sw.ExitBlock();
            }
          }
        },
        falseHandler);
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

    HandleMemberEndiannessAndAtPosition_(
        sw,
        member,
        _ => {
          if (!primitiveType.IsReadOnly) {
            sw.WriteLine(
                $"this.{member.Name} = {GetReadOneViaMethodText_(sourceSymbol, primitiveType)};");
          } else {
            sw.WriteLine($"{GetAssertOneViaMethodText_(
                primitiveType,
                $"this.{member.Name}")};");
          }
        },
        () => sw.WriteLine($"this.{member.Name} = default;"));
  }

  private static void ReadString_(
      ISourceWriter sw,
      ISchemaValueMember member) {
    HandleMemberEndianness_(
        sw,
        member,
        () => {
          var stringType =
              Asserts.CastNonnull(member.MemberType as IStringType);

          var falseHandler
              = () => {
                  var defaultValue
                      = member.MemberType.TypeInfo.IsNullable ? "null" : "\"\"";
                  sw.WriteLine($"this.{member.Name} = {defaultValue};");
                };

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
            HandleAtPosition_(
                sw,
                member,
                _ => {
                  if (stringType.LengthSourceType ==
                      StringLengthSourceType.NULL_TERMINATED) {
                    sw.WriteLine(
                        $"{READER}.AssertStringNT({encodingTypeWithComma}this.{member.Name});");
                  } else {
                    sw.WriteLine(
                        $"{READER}.AssertString({encodingTypeWithComma}this.{member.Name});");
                  }
                },
                falseHandler);

            return;
          }

          if (stringType.LengthSourceType ==
              StringLengthSourceType.NULL_TERMINATED) {
            HandleAtPosition_(
                sw,
                member,
                _ => {
                  sw.WriteLine(
                      $"this.{member.Name} = {READER}.ReadStringNT({encodingType});");
                },
                falseHandler);

            return;
          }

          var readMethod = stringType.IsNullTerminated
              ? "ReadStringNT"
              : "ReadString";

          if (stringType.LengthSourceType == StringLengthSourceType.CONST) {
            var constLength = stringType.ConstLength;
            if (constLength > 0) {
              HandleAtPosition_(
                  sw,
                  member,
                  _ => {
                    sw.WriteLine(
                        $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}{constLength});");
                  },
                  falseHandler);
            } else {
              falseHandler();
            }

            return;
          }

          if (stringType.LengthSourceType ==
              StringLengthSourceType.IMMEDIATE_VALUE) {
            var readType =
                SchemaGeneratorUtil.GetIntLabel(
                    stringType.ImmediateLengthType);
            HandleAtPosition_(
                sw,
                member,
                enteredBlock => {
                  if (!enteredBlock) {
                    sw.EnterBlock();
                  }

                  sw.WriteLine($"var l = {READER}.Read{readType}();")
                    .WriteLine(
                        $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}l);");

                  if (!enteredBlock) {
                    sw.ExitBlock();
                  }
                },
                falseHandler);
            return;
          }

          if (stringType.LengthSourceType ==
              StringLengthSourceType.OTHER_MEMBER) {
            var lengthName = stringType.LengthMember.Name;
            HandleAtPosition_(
                sw,
                member,
                $"{lengthName} > 0",
                _ => {
                  sw.WriteLine(
                      $"this.{member.Name} = {READER}.{readMethod}({encodingTypeWithComma}{lengthName});");
                },
                falseHandler);
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

    HandleMemberEndiannessAndAtPosition_(
        sw,
        member,
        _ => {
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
        },
        () => sw.WriteLine($"this.{member.Name} = default;"));
  }

  private static void ReadKnownStruct_(
      ISourceWriter sw,
      ITypeSymbol sourceSymbol,
      IKnownStructMemberType knownStructMemberType,
      ISchemaValueMember member) {
    HandleMemberEndiannessAndAtPosition_(
        sw,
        member,
        _ => {
          sw.WriteLine(
              $"this.{member.Name} = {GetReadOneViaMethodText_(sourceSymbol, knownStructMemberType)};");

          // TODO: Handle assertions
          if (knownStructMemberType.IsReadOnly) {
            throw new NotImplementedException();
          }
        },
        () => sw.WriteLine($"this.{member.Name} = default;"));
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
      HandleAtPosition_(
          sw,
          member,
          _ => {
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
          });

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

      HandleAtPosition_(
          sw,
          member,
          !hasConstLength && !isImmediate ? $"{lengthName} > 0" : null,
          _ => {
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

            BinarySchemaReaderGenerator.ReadIntoArray_(
                sw,
                sourceSymbol,
                member);
          },
          falseHandler);
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
        });
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