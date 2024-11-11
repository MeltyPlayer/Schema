using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using schema.binary.attributes;
using schema.binary.parser;
using schema.binary.parser.asserts;
using schema.util.symbols;


namespace schema.binary;

public interface IBinarySchemaContainerParser {
  IBinarySchemaContainer ParseContainer(INamedTypeSymbol containerSymbol);
}

public interface IBinarySchemaContainer {
  IReadOnlyList<Diagnostic> Diagnostics { get; }
  INamedTypeSymbol TypeSymbol { get; }
  IReadOnlyList<ISchemaMember> Members { get; }
  bool LocalPositions { get; }
  Endianness? Endianness { get; }
}

public interface ISchemaMember {
  string Name { get; }
}

public interface ISchemaMethodMember : ISchemaMember { }

public interface ISchemaValueMember : ISchemaMember {
  IMemberType MemberType { get; }
  bool IsSkipped { get; }
  AlignAttribute? Align { get; }
  IIfBooleanAttribute? IfBoolean { get; }
  IOffset? Offset { get; }
  bool IsPosition { get; }
  Endianness? Endianness { get; }
  bool TrackStartAndEnd { get; }
}

public interface IMemberType {
  ITypeInfo TypeInfo { get; }
  ITypeSymbol TypeSymbol { get; }
  bool IsReadOnly { get; }
}

public interface IPrimitiveMemberType : IMemberType {
  SchemaPrimitiveType PrimitiveType { get; }
  bool UseAltFormat { get; }
  SchemaNumberType AltFormat { get; }

  bool SizeOfStream { get; }
  IMemberReference<string>[]? LengthOfStringMembers { get; }
  IMemberReference[]? LengthOfSequenceMembers { get; }
  IChain<IAccessChainNode>? AccessChainToSizeOf { get; }
  IPointerToAttribute? PointerToAttribute { get; }
}

public interface IContainerMemberType : IMemberType {
  bool IsChild { get; }
}

public interface IGenericMemberType : IMemberType {
  IMemberType? ConstraintType { get; }
}

public interface IOffset {
  ISchemaValueMember OffsetName { get; }
  long? NullValue { get; }
}

public interface IStringType : IMemberType {
  // TODO: Support char format?
  /// <summary>
  ///   Whether the string has a set length. This is required for non-null
  ///   terminated strings.
  ///
  ///   If this is set on a null-terminated string, this will function as
  ///   the max length (minus one, since the null-terminator is included).
  /// </summary>
  StringLengthSourceType LengthSourceType { get; }

  StringEncodingType EncodingType { get; }
  bool IsNullTerminated { get; }
  SchemaIntegerType ImmediateLengthType { get; }
  IMemberReference? LengthMember { get; }
  int ConstLength { get; }
}

public enum SequenceType {
  MUTABLE_ARRAY,
  IMMUTABLE_ARRAY,
  MUTABLE_LIST,
  READ_ONLY_LIST,
  MUTABLE_SEQUENCE,
  CONST_LENGTH_MUTABLE_SEQUENCE,
  READ_ONLY_SEQUENCE,
}

public static class SequenceTypeExtensions {
  public static bool IsConstLength(this SequenceType sequenceType)
    => sequenceType switch {
        SequenceType.MUTABLE_ARRAY                 => true,
        SequenceType.IMMUTABLE_ARRAY               => true,
        SequenceType.MUTABLE_LIST                  => false,
        SequenceType.READ_ONLY_LIST                => true,
        SequenceType.MUTABLE_SEQUENCE              => false,
        SequenceType.CONST_LENGTH_MUTABLE_SEQUENCE => true,
        SequenceType.READ_ONLY_SEQUENCE            => true,
    };

  public static bool IsReadOnly(this SequenceType sequenceType)
    => sequenceType switch {
        SequenceType.MUTABLE_ARRAY                 => false,
        SequenceType.IMMUTABLE_ARRAY               => true,
        SequenceType.MUTABLE_LIST                  => false,
        SequenceType.READ_ONLY_LIST                => true,
        SequenceType.MUTABLE_SEQUENCE              => false,
        SequenceType.CONST_LENGTH_MUTABLE_SEQUENCE => false,
        SequenceType.READ_ONLY_SEQUENCE            => true,
    };

  public static bool IsArray(this SequenceType sequenceType)
    => sequenceType is SequenceType.MUTABLE_ARRAY
                       or SequenceType.IMMUTABLE_ARRAY;

  public static bool IsISequence(this SequenceType sequenceType)
    => sequenceType is SequenceType.MUTABLE_SEQUENCE
                       or SequenceType.CONST_LENGTH_MUTABLE_SEQUENCE
                       or SequenceType.READ_ONLY_SEQUENCE;
}

public enum SequenceLengthSourceType {
  UNSPECIFIED,
  IMMEDIATE_VALUE,
  OTHER_MEMBER,
  CONST_LENGTH,
  READ_ONLY,
  UNTIL_END_OF_STREAM,
}

public interface ISequenceMemberType : IMemberType {
  ISequenceTypeInfo SequenceTypeInfo { get; }

  SequenceLengthSourceType LengthSourceType { get; }
  SchemaIntegerType ImmediateLengthType { get; }
  ISchemaValueMember? LengthMember { get; }
  uint ConstLength { get; }

  IMemberType ElementType { get; }
}

public class BinarySchemaContainerParser : IBinarySchemaContainerParser {
  public IBinarySchemaContainer ParseContainer(
      INamedTypeSymbol containerSymbol) {
    var containerBetterSymbol = BetterSymbol.FromType(containerSymbol);

    // All of the types that contain the container need to be partial
    new PartialContainerAsserter(containerBetterSymbol)
        .AssertContainersArePartial(containerSymbol);

    if (containerSymbol.IsChild(out var parentSymbol)) {
      if (!parentSymbol.ContainsMemberWithType(containerSymbol)) {
        containerBetterSymbol.ReportDiagnostic(
            Rules.ChildTypeMustBeContainedInParent);
      }

      if (!parentSymbol.IsAtLeastAsBinaryConvertibleAs(containerSymbol)) {
        containerBetterSymbol.ReportDiagnostic(
            Rules.ParentBinaryConvertabilityMustSatisfyChild);
      }
    }

    var localPositions =
        containerSymbol.HasAttribute<LocalPositionsAttribute>();
    var containerEndianness =
        new EndiannessParser().GetEndianness(containerBetterSymbol);

    var typeInfoParser = new TypeInfoParser();
    var parsedMembers =
        typeInfoParser.ParseMembers(containerSymbol).ToArray();

    var members = new List<ISchemaMember>();
    foreach (var parsedMember in parsedMembers) {
      var (parseStatus, memberSymbol, _, _) = parsedMember;
      if (parseStatus ==
          TypeInfoParser.ParseStatus
                        .NOT_A_FIELD_OR_PROPERTY_OR_METHOD) {
        continue;
      }

      if (memberSymbol is IPropertySymbol {IsIndexer: true}) {
        continue;
      }

      var memberBetterSymbol = containerBetterSymbol.GetChild(memberSymbol);

      {
        var methodSymbol = memberSymbol as IMethodSymbol;
        var isMethod = methodSymbol != null;
        var hasRunAtReadTimeAttribute =
            memberBetterSymbol.HasAttribute<ReadLogicAttribute>();
        if (hasRunAtReadTimeAttribute) {
          if (isMethod) {
            if (methodSymbol.Parameters.Length == 1 &&
                ComparisonExtensions.IsType((ISymbol) methodSymbol
                                                .Parameters[0]
                                                .Type,
                                            typeof(IBinaryReader))) {
              members.Add(
                  new SchemaMethodMember { Name = methodSymbol.Name });
            } else {
              memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
            }
          } else {
            memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
          }
        }

        if (isMethod) {
          continue;
        }
      }

      bool isSkipped =
          memberBetterSymbol.HasAttribute<SkipAttribute>() ||
          (memberSymbol.Name == nameof(IChildOf<IBinaryConvertible>.Parent) &&
           parentSymbol != null) ||
          (containerSymbol.IsIndexed() &&
           memberSymbol.Name == nameof(IIndexed.Index));

      // Skips parent field for child types

      var field =
          !isSkipped
              ? this.ParseNonSkippedField_(
                  containerSymbol,
                  memberBetterSymbol,
                  parsedMember)
              : this.ParseSkippedField_(parsedMember);

      if (field != null) {
        members.Add(field);
      }
    }

    var schemaContainer = new BinarySchemaContainer {
        Diagnostics = containerBetterSymbol.Diagnostics,
        TypeSymbol = containerSymbol,
        Members = members,
        LocalPositions = localPositions,
        Endianness = containerEndianness,
    };

    // Hooks up size of dependencies.
    var containerByNamedTypeSymbol =
        new Dictionary<INamedTypeSymbol, IBinarySchemaContainer>();
    containerByNamedTypeSymbol[containerSymbol] = schemaContainer;
    {
      var sizeOfMemberInBytesDependencyFixer =
          new WSizeOfMemberInBytesDependencyFixer();
      foreach (var member in members.OfType<ISchemaValueMember>()) {
        if (member.MemberType is IPrimitiveMemberType
            primitiveMemberType) {
          if (primitiveMemberType.AccessChainToSizeOf != null) {
            sizeOfMemberInBytesDependencyFixer.AddDependenciesForContainer(
                containerByNamedTypeSymbol,
                primitiveMemberType.AccessChainToSizeOf);
          }

          var pointerToAttribute = primitiveMemberType.PointerToAttribute;
          if (pointerToAttribute != null) {
            sizeOfMemberInBytesDependencyFixer.AddDependenciesForContainer(
                containerByNamedTypeSymbol,
                pointerToAttribute.AccessChainToOtherMember);
          }
        }
      }
    }

    return schemaContainer;
  }

  private ISchemaValueMember? ParseNonSkippedField_(
      INamedTypeSymbol containerTypeSymbol,
      IBetterSymbol memberBetterSymbol,
      (TypeInfoParser.ParseStatus, ISymbol, ITypeSymbol, ITypeInfo)
          parsedMember
  ) {
    var (parseStatus, memberSymbol, memberTypeSymbol, memberTypeInfo) =
        parsedMember;

    if (parseStatus == TypeInfoParser.ParseStatus.NOT_IMPLEMENTED) {
      memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
      return null;
    }

    // Makes sure the member is serializable
    if (memberTypeInfo is IContainerTypeInfo) {
      if (!memberTypeInfo.TypeSymbol.IsAtLeastAsBinaryConvertibleAs(
              containerTypeSymbol)) {
        memberBetterSymbol.ReportDiagnostic(
            Rules.ContainerMemberBinaryConvertabilityNeedsToSatisfyParent);
        return null;
      }
    }

    var memberEndianness =
        new EndiannessParser().GetEndianness(memberBetterSymbol);

    // Gets the type of the current member
    var memberType =
        MemberReferenceUtil.WrapTypeInfoWithMemberType(memberTypeInfo);

    var attributeParsers = new IAttributeParser[] {
        new SequenceLengthSourceParser(),
        new WLengthOfSequenceParser(),
        new WLengthOfStringParser(),
        new WPointerToParser(),
        new WSizeOfMemberInBytesParser(),
    };

    foreach (var attributeParser in attributeParsers) {
      attributeParser.ParseIntoMemberType(memberBetterSymbol,
                                          memberTypeInfo,
                                          memberType);
    }

    // Get attributes
    var align = memberBetterSymbol.GetAttribute<AlignAttribute>();

    {
      var sizeOfStreamAttribute =
          memberBetterSymbol.GetAttribute<WSizeOfStreamInBytesAttribute>();
      if (sizeOfStreamAttribute != null) {
        if (memberTypeInfo is IIntegerTypeInfo &&
            memberType is PrimitiveMemberType primitiveMemberType) {
          primitiveMemberType.SizeOfStream = true;
        } else {
          memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
        }
      }
    }

    var isPosition = false;
    {
      var positionAttribute =
          memberBetterSymbol
              .GetAttribute<RPositionRelativeToStreamAttribute>();
      if (positionAttribute != null) {
        isPosition = true;
        if (memberTypeInfo is not IIntegerTypeInfo {
                IntegerType: SchemaIntegerType.INT64
            }) {
          memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
        }
      }
    }

    var ifBooleanAttribute =
        (IIfBooleanAttribute?)
        memberBetterSymbol.GetAttribute<IfBooleanAttribute>() ??
        memberBetterSymbol.GetAttribute<RIfBooleanAttribute>();
    if (ifBooleanAttribute != null && !memberTypeInfo.IsNullable) {
      memberBetterSymbol.ReportDiagnostic(Rules.IfBooleanNeedsNullable);
    }

    IOffset? offset = null;
    {
      var atPositionAttribute =
          (IAtPositionAttribute?) memberBetterSymbol
              .GetAttribute<RAtPositionAttribute>() ??
          memberBetterSymbol.GetAttribute<RAtPositionOrNullAttribute>();

      if (atPositionAttribute != null) {
        var offsetName = atPositionAttribute.OffsetName;
        SymbolTypeUtil.GetMemberRelativeToAnother(
            memberBetterSymbol,
            containerTypeSymbol,
            offsetName,
            memberSymbol.Name,
            true,
            out _,
            out _,
            out var offsetTypeInfo);

        offset = new Offset {
            OffsetName = new SchemaValueMember {
                Name = offsetName,
                MemberType =
                    MemberReferenceUtil.WrapTypeInfoWithMemberType(
                        offsetTypeInfo),
            },
            NullValue = atPositionAttribute.NullValue,
        };
      }
    }

    new SupportedElementTypeAsserter()
        .AssertElementTypesAreSupported(memberBetterSymbol,
                                        containerTypeSymbol,
                                        memberType);

    {
      IMemberType? targetMemberType;
      if (memberType is ISequenceMemberType sequenceMember) {
        targetMemberType = sequenceMember.ElementType;
      } else {
        targetMemberType = memberType;
      }

      var targetPrimitiveType = SchemaPrimitiveType.UNDEFINED;
      if (targetMemberType is IPrimitiveMemberType primitiveType) {
        targetPrimitiveType = primitiveType.PrimitiveType;
      }

      // TODO: Apply this to element type as well
      var formatNumberType = SchemaNumberType.UNDEFINED;
      var formatIntegerType = SchemaIntegerType.UNDEFINED;

      if (targetMemberType.TypeSymbol.IsEnum(out var underlyingType)) {
        formatIntegerType = underlyingType;
      }

      {
        var numberFormatAttribute =
            memberBetterSymbol.GetAttribute<NumberFormatAttribute>();
        if (numberFormatAttribute != null) {
          formatNumberType = numberFormatAttribute.NumberType;

          var canPrimitiveTypeBeReadAsNumber =
              targetPrimitiveType.CanBeReadAsNumber();
          if (!(targetMemberType is PrimitiveMemberType &&
                canPrimitiveTypeBeReadAsNumber)) {
            memberBetterSymbol.ReportDiagnostic(
                Rules.UnexpectedAttribute);
          }
        }
      }

      {
        var integerFormatAttribute =
            memberBetterSymbol.GetAttribute<IntegerFormatAttribute>();
        if (integerFormatAttribute != null) {
          formatIntegerType = integerFormatAttribute.IntegerType;

          var canPrimitiveTypeBeReadAsInteger =
              targetPrimitiveType.CanBeReadAsInteger();
          if (!(targetMemberType is PrimitiveMemberType &&
                canPrimitiveTypeBeReadAsInteger)) {
            memberBetterSymbol.ReportDiagnostic(Rules.UnexpectedAttribute);
          }
        }
      }

      if (formatNumberType == SchemaNumberType.UNDEFINED &&
          formatIntegerType != SchemaIntegerType.UNDEFINED) {
        formatNumberType = formatIntegerType.AsNumberType();
      }

      if (targetMemberType is PrimitiveMemberType primitiveMemberType) {
        if (formatNumberType != SchemaNumberType.UNDEFINED) {
          primitiveMemberType.UseAltFormat = true;
          primitiveMemberType.AltFormat = formatNumberType;
        } else if (targetPrimitiveType == SchemaPrimitiveType.ENUM) {
          memberBetterSymbol.ReportDiagnostic(Rules.EnumNeedsIntegerFormat);
        } else if (targetPrimitiveType == SchemaPrimitiveType.BOOLEAN) {
          memberBetterSymbol.ReportDiagnostic(
              Rules.BooleanNeedsIntegerFormat);
        }
      }

      // Checks if the member is a child of the current type
      {
        if (targetMemberType is ContainerMemberType containerMemberType) {
          if (targetMemberType.TypeSymbol.IsChild(
                  out var expectedParentTypeSymbol)) {
            if (expectedParentTypeSymbol.IsSameAs(containerTypeSymbol)) {
              containerMemberType.IsChild = true;
            } else {
              memberBetterSymbol.ReportDiagnostic(
                  Rules.ChildTypeCanOnlyBeContainedInParent);
            }
          }
        }
      }
    }

    {
      // TODO: Implement this, support strings in arrays?
      var stringLengthSourceAttribute =
          (IStringLengthSourceAttribute?)
          memberBetterSymbol.GetAttribute<StringLengthSourceAttribute>() ??
          memberBetterSymbol.GetAttribute<RStringLengthSourceAttribute>();
      var isNullTerminatedString = memberBetterSymbol
          .HasAttribute<NullTerminatedStringAttribute>();
      var hasStringLengthAttribute =
          stringLengthSourceAttribute != null || isNullTerminatedString;


      var encodingTypeAttribute =
          memberBetterSymbol.GetAttribute<StringEncodingAttribute>();


      if (hasStringLengthAttribute || encodingTypeAttribute != null) {
        if (memberType is StringType stringType) {
          if (memberTypeInfo.IsReadOnly && hasStringLengthAttribute) {
            memberBetterSymbol.ReportDiagnostic(Rules.UnexpectedAttribute);
          }

          stringType.EncodingType = encodingTypeAttribute?.EncodingType ??
                                    StringEncodingType.ASCII;
          stringType.IsNullTerminated = isNullTerminatedString;
          stringType.LengthSourceType =
              stringLengthSourceAttribute?.Method ??
              StringLengthSourceType.NULL_TERMINATED;
          switch (stringType.LengthSourceType) {
            case StringLengthSourceType.CONST: {
              stringType.ConstLength =
                  stringLengthSourceAttribute!.ConstLength;
              break;
            }
            case StringLengthSourceType.NULL_TERMINATED: {
              break;
            }
            case StringLengthSourceType.IMMEDIATE_VALUE: {
              stringType.ImmediateLengthType =
                  stringLengthSourceAttribute.ImmediateLengthType;
              break;
            }
            case StringLengthSourceType.OTHER_MEMBER: {
              stringType.LengthMember =
                  stringLengthSourceAttribute.OtherMember;
              break;
            }
            default: {
              memberBetterSymbol.ReportDiagnostic(Rules.NotSupported);
              break;
            }
          }
        } else {
          memberBetterSymbol.ReportDiagnostic(Rules.UnexpectedAttribute);
        }
      }
    }

    return new SchemaValueMember {
        Name = memberSymbol.Name,
        MemberType = memberType,
        IsSkipped = false,
        Align = align,
        IfBoolean = ifBooleanAttribute,
        Offset = offset,
        IsPosition = isPosition,
        Endianness = memberEndianness,
    };
  }

  private ISchemaValueMember? ParseSkippedField_(
      (TypeInfoParser.ParseStatus, ISymbol, ITypeSymbol, ITypeInfo)
          parsedMember
  ) {
    var (parseStatus, memberSymbol, _, memberTypeInfo) = parsedMember;
    if (parseStatus == TypeInfoParser.ParseStatus.NOT_IMPLEMENTED) {
      return null;
    }

    // Gets the type of the current member
    var memberType =
        MemberReferenceUtil.WrapTypeInfoWithMemberType(memberTypeInfo);

    return new SchemaValueMember {
        Name = memberSymbol.Name, MemberType = memberType, IsSkipped = true,
    };
  }


  private class BinarySchemaContainer : IBinarySchemaContainer {
    public IReadOnlyList<Diagnostic> Diagnostics { get; set; }
    public INamedTypeSymbol TypeSymbol { get; set; }
    public IReadOnlyList<ISchemaMember> Members { get; set; }
    public bool LocalPositions { get; set; }
    public Endianness? Endianness { get; set; }
  }

  public class SchemaMethodMember : ISchemaMethodMember {
    public string Name { get; set; }
  }

  public class SchemaValueMember : ISchemaValueMember {
    public string Name { get; set; }
    public IMemberType MemberType { get; set; }
    public bool IsSkipped { get; set; }
    public AlignAttribute? Align { get; set; }
    public IIfBooleanAttribute? IfBoolean { get; set; }
    public IOffset Offset { get; set; }
    public bool IsPosition { get; set; }
    public Endianness? Endianness { get; set; }
    public bool TrackStartAndEnd { get; set; }
  }

  public class PrimitiveMemberType : IPrimitiveMemberType {
    public IPrimitiveTypeInfo PrimitiveTypeInfo { get; set; }
    public ITypeInfo TypeInfo => PrimitiveTypeInfo;
    public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
    public bool IsReadOnly => this.TypeInfo.IsReadOnly;

    public SchemaPrimitiveType PrimitiveType
      => this.PrimitiveTypeInfo.PrimitiveType;

    public bool UseAltFormat { get; set; }
    public SchemaNumberType AltFormat { get; set; }

    public bool SizeOfStream { get; set; }
    public IMemberReference<string>[]? LengthOfStringMembers { get; set; }
    public IMemberReference[]? LengthOfSequenceMembers { get; set; }
    public IChain<IAccessChainNode>? AccessChainToSizeOf { get; set; }
    public IPointerToAttribute? PointerToAttribute { get; set; }
  }

  public class ContainerMemberType : IContainerMemberType {
    public IContainerTypeInfo ContainerTypeInfo { get; set; }
    public ITypeInfo TypeInfo => this.ContainerTypeInfo;
    public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
    public bool IsReadOnly => this.TypeInfo.IsReadOnly;

    public bool IsChild { get; set; }
  }

  public class GenericMemberType : IGenericMemberType {
    public IMemberType? ConstraintType { get; set; }
    public IGenericTypeInfo GenericTypeInfo { get; set; }
    public ITypeInfo TypeInfo => GenericTypeInfo;
    public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
    public bool IsReadOnly => this.TypeInfo.IsReadOnly;
  }

  public class Offset : IOffset {
    public ISchemaValueMember OffsetName { get; set; }
    public long? NullValue { get; set; }
  }

  public class StringType : IStringType {
    public ITypeInfo TypeInfo { get; set; }
    public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
    public bool IsReadOnly => this.TypeInfo.IsReadOnly;

    public StringEncodingType EncodingType { get; set; }
    public bool IsNullTerminated { get; set; }
    public StringLengthSourceType LengthSourceType { get; set; }
    public SchemaIntegerType ImmediateLengthType { get; set; }
    public IMemberReference? LengthMember { get; set; }
    public int ConstLength { get; set; }
  }

  public class SequenceMemberType : ISequenceMemberType {
    public ISequenceTypeInfo SequenceTypeInfo { get; set; }
    public ITypeInfo TypeInfo => SequenceTypeInfo;
    public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
    public bool IsReadOnly => this.TypeInfo.IsReadOnly;

    public SequenceLengthSourceType LengthSourceType { get; set; }
    public SchemaIntegerType ImmediateLengthType { get; set; }
    public ISchemaValueMember? LengthMember { get; set; }
    public uint ConstLength { get; set; }

    public IMemberType ElementType { get; set; }
  }
}