using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using System.IO;

using schema.binary.attributes;
using schema.binary.parser;
using schema.binary.parser.asserts;


namespace schema.binary {
  public interface IBinarySchemaStructureParser {
    IBinarySchemaStructure ParseStructure(INamedTypeSymbol symbol);
  }

  public interface IBinarySchemaStructure {
    IList<Diagnostic> Diagnostics { get; }
    INamedTypeSymbol TypeSymbol { get; }
    IReadOnlyList<ISchemaMember> Members { get; }
    Endianness? Endianness { get; }
  }

  public enum SchemaPrimitiveType {
    UNDEFINED,

    BOOLEAN,

    SBYTE,
    BYTE,
    INT16,
    UINT16,
    INT24,
    UINT24,
    INT32,
    UINT32,
    INT64,
    UINT64,

    HALF,
    SINGLE,
    DOUBLE,

    SN8,
    UN8,

    SN16,
    UN16,

    CHAR,

    ENUM,
  }

  public interface ISchemaMember {
    string Name { get; }
  }

  public interface ISchemaMethodMember : ISchemaMember { }

  public interface ISchemaValueMember : ISchemaMember {
    IMemberType MemberType { get; }
    bool IsIgnored { get; }
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
    IMemberReference<string>? LengthOfStringMember { get; }
    IMemberReference? LengthOfSequenceMember { get; }
    IChain<IAccessChainNode>? AccessChainToSizeOf { get; }
    IChain<IAccessChainNode>? AccessChainToPointer { get; }
  }

  public interface IStructureMemberType : IMemberType {
    IStructureTypeInfo StructureTypeInfo { get; }
    bool IsReferenceType { get; }
    bool IsChild { get; }
    bool IsStruct { get; }
  }

  public interface IGenericMemberType : IMemberType {
    IMemberType ConstraintType { get; }
    IGenericTypeInfo GenericTypeInfo { get; }
  }

  public interface IOffset {
    ISchemaValueMember OffsetName { get; }
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
    READ_ONLY_SEQUENCE,
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

  public class BinarySchemaStructureParser : IBinarySchemaStructureParser {
    public IBinarySchemaStructure ParseStructure(
        INamedTypeSymbol structureSymbol) {
      var diagnostics = new List<Diagnostic>();

      // All of the types that contain the structure need to be partial
      new PartialContainerAsserter(diagnostics).AssertContainersArePartial(
          structureSymbol);

      var iChildOfParser = new ChildOfParser(diagnostics);
      var parentTypeSymbol =
          iChildOfParser.GetParentTypeSymbolOf(structureSymbol);
      if (parentTypeSymbol != null) {
        iChildOfParser.AssertParentContainsChild(
            parentTypeSymbol,
            structureSymbol);
        iChildOfParser.AssertParentBinaryConvertabilityMatchesChild(
            parentTypeSymbol,
            structureSymbol);
      }

      var structureEndianness =
          new EndiannessParser().GetEndianness(diagnostics, structureSymbol);

      var typeInfoParser = new TypeInfoParser();
      var parsedMembers =
          typeInfoParser.ParseMembers(structureSymbol).ToArray();

      var members = new List<ISchemaMember>();
      foreach (var parsedMember in parsedMembers) {
        var (parseStatus, memberSymbol, _) = parsedMember;
        if (parseStatus == TypeInfoParser.ParseStatus
                                         .NOT_A_FIELD_OR_PROPERTY_OR_METHOD) {
          continue;
        }

        {
          var methodSymbol = memberSymbol as IMethodSymbol;
          var isMethod = methodSymbol != null;
          var hasRunAtReadTimeAttribute =
              memberSymbol.HasAttribute<ReadTimeLogicAttribute>(diagnostics);
          if (hasRunAtReadTimeAttribute) {
            if (isMethod) {
              if (methodSymbol.Parameters.Length == 1 && methodSymbol
                      .Parameters[0]
                      .Type
                      .IsExactlyType(typeof(IEndianBinaryReader))) {
                members.Add(
                    new SchemaMethodMember { Name = methodSymbol.Name });
              } else {
                diagnostics.Add(
                    Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
              }
            } else {
              diagnostics.Add(
                  Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
            }
          }

          if (isMethod) {
            continue;
          }
        }

        bool isIgnored =
            memberSymbol.HasAttribute<IgnoreAttribute>(diagnostics) ||
            (memberSymbol.Name == nameof(IChildOf<IBinaryConvertible>.Parent)
             && parentTypeSymbol != null);

        // Skips parent field for child types

        var field =
            !isIgnored
                ? this.ParseNonIgnoredField_(
                    diagnostics,
                    structureSymbol,
                    parsedMember)
                : this.ParseIgnoredField_(parsedMember);

        if (field != null) {
          members.Add(field);
        }
      }

      var schemaStructure = new BinarySchemaStructure {
          Diagnostics = diagnostics,
          TypeSymbol = structureSymbol,
          Members = members,
          Endianness = structureEndianness,
      };

      // Hooks up size of dependencies.
      var structureByNamedTypeSymbol =
          new Dictionary<INamedTypeSymbol, IBinarySchemaStructure>();
      structureByNamedTypeSymbol[structureSymbol] = schemaStructure;
      {
        var sizeOfMemberInBytesDependencyFixer =
            new WSizeOfMemberInBytesDependencyFixer();
        foreach (var member in members.OfType<ISchemaValueMember>()) {
          if (member.MemberType is IPrimitiveMemberType
              primitiveMemberType) {
            if (primitiveMemberType.AccessChainToSizeOf != null) {
              sizeOfMemberInBytesDependencyFixer.AddDependenciesForStructure(
                  structureByNamedTypeSymbol,
                  primitiveMemberType.AccessChainToSizeOf);
            }

            if (primitiveMemberType.AccessChainToPointer != null) {
              sizeOfMemberInBytesDependencyFixer.AddDependenciesForStructure(
                  structureByNamedTypeSymbol,
                  primitiveMemberType.AccessChainToPointer);
            }
          }
        }
      }

      return schemaStructure;
    }

    private ISchemaValueMember? ParseNonIgnoredField_(
        IList<Diagnostic> diagnostics,
        INamedTypeSymbol structureSymbol,
        (TypeInfoParser.ParseStatus, ISymbol, ITypeInfo) parsedMember
    ) {
      var (parseStatus, memberSymbol, memberTypeInfo) = parsedMember;

      if (parseStatus == TypeInfoParser.ParseStatus.NOT_IMPLEMENTED) {
        diagnostics.Add(
            Rules.CreateDiagnostic(
                memberSymbol,
                Rules.NotSupported));
        return null;
      }

      // Makes sure the member is serializable
      {
        var isDeserializable = structureSymbol.IsBinaryDeserializable();
        var isSerializable = structureSymbol.IsBinarySerializable();

        if (memberTypeInfo is IStructureTypeInfo) {
          var isMemberDeserializable =
              memberTypeInfo.TypeSymbol.IsBinaryDeserializable();
          var isMemberSerializable =
              memberTypeInfo.TypeSymbol.IsBinarySerializable();

          if ((isDeserializable && !isMemberDeserializable) ||
              (isSerializable && !isMemberSerializable)) {
            diagnostics.Add(
                Rules.CreateDiagnostic(
                    memberSymbol,
                    Rules
                        .StructureMemberBinaryConvertabilityNeedsToSatisfyParent));
            return null;
          }
        }
      }

      var memberEndianness =
          new EndiannessParser().GetEndianness(diagnostics, memberSymbol);

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
        attributeParser.ParseIntoMemberType(diagnostics,
                                            memberSymbol,
                                            memberTypeInfo,
                                            memberType);
      }

      // Get attributes
      var align = memberSymbol.GetAttribute<AlignAttribute>(diagnostics);

      {
        var sizeOfStreamAttribute =
            SymbolTypeUtil.GetAttribute<WSizeOfStreamInBytesAttribute>(
                diagnostics,
                memberSymbol);
        if (sizeOfStreamAttribute != null) {
          if (memberTypeInfo is IIntegerTypeInfo &&
              memberType is BinarySchemaStructureParser.PrimitiveMemberType
                  primitiveMemberType) {
            primitiveMemberType.SizeOfStream = true;
          } else {
            diagnostics.Add(
                Rules.CreateDiagnostic(memberSymbol, Rules.NotSupported));
          }
        }
      }

      var isPosition = false;
      {
        var positionAttribute =
            SymbolTypeUtil.GetAttribute<RPositionRelativeToStreamAttribute>(
                diagnostics,
                memberSymbol);
        if (positionAttribute != null) {
          isPosition = true;
          if (memberTypeInfo is not IIntegerTypeInfo {
                  IntegerType: SchemaIntegerType.INT64
              }) {
            diagnostics.Add(
                Rules.CreateDiagnostic(
                    memberSymbol,
                    Rules.NotSupported));
          }
        }
      }

      var ifBooleanAttribute =
          (IIfBooleanAttribute?)
          SymbolTypeUtil.GetAttribute<IfBooleanAttribute>(
              diagnostics,
              memberSymbol) ??
          SymbolTypeUtil.GetAttribute<RIfBooleanAttribute>(
              diagnostics,
              memberSymbol);
      if (ifBooleanAttribute != null && !memberTypeInfo.IsNullable) {
        diagnostics.Add(
            Rules.CreateDiagnostic(memberSymbol, Rules.IfBooleanNeedsNullable));
      }

      IOffset? offset = null;
      {
        var offsetAttribute =
            SymbolTypeUtil.GetAttribute<RAtOffsetAttribute>(
                diagnostics,
                memberSymbol);

        if (offsetAttribute != null) {
          var offsetName = offsetAttribute.OffsetName;
          SymbolTypeUtil.GetMemberRelativeToAnother(
              diagnostics,
              structureSymbol,
              offsetName,
              memberSymbol.Name,
              true,
              out _,
              out var offsetTypeInfo);

          offset = new Offset {
              OffsetName = new SchemaValueMember {
                  Name = offsetName,
                  MemberType =
                      MemberReferenceUtil.WrapTypeInfoWithMemberType(
                          offsetTypeInfo),
              }
          };
        }
      }

      new SupportedElementTypeAsserter(diagnostics)
          .AssertElementTypesAreSupported(memberSymbol, memberType);

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

        if (targetPrimitiveType == SchemaPrimitiveType.ENUM) {
          var enumNamedTypeSymbol =
              targetMemberType.TypeSymbol as INamedTypeSymbol;
          var underlyingType = enumNamedTypeSymbol!.EnumUnderlyingType;

          formatIntegerType =
              SchemaPrimitiveTypesUtil.GetIntegerTypeFromTypeSymbol(
                  underlyingType);
        }

        {
          var numberFormatAttribute =
              SymbolTypeUtil
                  .GetAttribute<NumberFormatAttribute>(
                      diagnostics,
                      memberSymbol);
          if (numberFormatAttribute != null) {
            formatNumberType = numberFormatAttribute.NumberType;

            var canPrimitiveTypeBeReadAsNumber =
                SchemaPrimitiveTypesUtil.CanPrimitiveTypeBeReadAsNumber(
                    targetPrimitiveType);
            if (!(targetMemberType is PrimitiveMemberType &&
                  canPrimitiveTypeBeReadAsNumber)) {
              diagnostics.Add(
                  Rules.CreateDiagnostic(memberSymbol,
                                         Rules.UnexpectedAttribute));
            }
          }
        }

        {
          var integerFormatAttribute =
              SymbolTypeUtil
                  .GetAttribute<IntegerFormatAttribute>(
                      diagnostics,
                      memberSymbol);
          if (integerFormatAttribute != null) {
            formatIntegerType = integerFormatAttribute.IntegerType;

            var canPrimitiveTypeBeReadAsInteger =
                SchemaPrimitiveTypesUtil.CanPrimitiveTypeBeReadAsInteger(
                    targetPrimitiveType);
            if (!(targetMemberType is PrimitiveMemberType &&
                  canPrimitiveTypeBeReadAsInteger)) {
              diagnostics.Add(
                  Rules.CreateDiagnostic(memberSymbol,
                                         Rules.UnexpectedAttribute));
            }
          }
        }

        if (formatNumberType == SchemaNumberType.UNDEFINED &&
            formatIntegerType != SchemaIntegerType.UNDEFINED) {
          formatNumberType =
              SchemaPrimitiveTypesUtil
                  .ConvertIntToNumber(formatIntegerType);
        }

        if (targetMemberType is PrimitiveMemberType primitiveMemberType) {
          if (formatNumberType != SchemaNumberType.UNDEFINED) {
            primitiveMemberType.UseAltFormat = true;
            primitiveMemberType.AltFormat = formatNumberType;
          } else if (targetPrimitiveType == SchemaPrimitiveType.ENUM) {
            diagnostics.Add(
                Rules.CreateDiagnostic(memberSymbol,
                                       Rules.EnumNeedsIntegerFormat));
          } else if (targetPrimitiveType == SchemaPrimitiveType.BOOLEAN) {
            diagnostics.Add(
                Rules.CreateDiagnostic(memberSymbol,
                                       Rules.BooleanNeedsIntegerFormat));
          }
        }

        // Checks if the member is a child of the current type
        {
          if (targetMemberType is StructureMemberType structureMemberType) {
            var expectedParentTypeSymbol =
                new ChildOfParser(diagnostics).GetParentTypeSymbolOf(
                    structureMemberType.StructureTypeInfo.NamedTypeSymbol);
            if (expectedParentTypeSymbol != null) {
              if (expectedParentTypeSymbol.Equals(structureSymbol)) {
                structureMemberType.IsChild = true;
              } else {
                diagnostics.Add(Rules.CreateDiagnostic(
                                    memberSymbol,
                                    Rules
                                        .ChildTypeCanOnlyBeContainedInParent));
              }
            }
          }
        }
      }

      {
        // TODO: Implement this, support strings in arrays?
        var stringLengthSourceAttribute =
            (IStringLengthSourceAttribute?) SymbolTypeUtil
                .GetAttribute<StringLengthSourceAttribute>(
                    diagnostics,
                    memberSymbol) ?? SymbolTypeUtil
                .GetAttribute<RStringLengthSourceAttribute>(
                    diagnostics,
                    memberSymbol);
        var isNullTerminatedString =
            memberSymbol.HasAttribute<NullTerminatedStringAttribute>(
                diagnostics);

        if (stringLengthSourceAttribute != null || isNullTerminatedString) {
          if (memberType is StringType stringType) {
            if (memberTypeInfo.IsReadOnly) {
              diagnostics.Add(
                  Rules.CreateDiagnostic(memberSymbol,
                                         Rules.UnexpectedAttribute));
            }

            stringType.IsNullTerminated = isNullTerminatedString;
            stringType.LengthSourceType =
                stringLengthSourceAttribute?.Method
                ?? StringLengthSourceType.NULL_TERMINATED;
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
                diagnostics.Add(
                    Rules.CreateDiagnostic(memberSymbol,
                                           Rules.NotSupported));
                break;
              }
            }
          } else {
            diagnostics.Add(
                Rules.CreateDiagnostic(memberSymbol,
                                       Rules.UnexpectedAttribute));
          }
        }
      }

      return new SchemaValueMember {
          Name = memberSymbol.Name,
          MemberType = memberType,
          IsIgnored = false,
          Align = align,
          IfBoolean = ifBooleanAttribute,
          Offset = offset,
          IsPosition = isPosition,
          Endianness = memberEndianness,
      };
    }

    private ISchemaValueMember ParseIgnoredField_(
        (TypeInfoParser.ParseStatus, ISymbol, ITypeInfo) parsedMember
    ) {
      var (_, memberSymbol, memberTypeInfo) = parsedMember;

      // Gets the type of the current member
      var memberType =
          MemberReferenceUtil.WrapTypeInfoWithMemberType(memberTypeInfo);

      return new SchemaValueMember {
          Name = memberSymbol.Name, MemberType = memberType, IsIgnored = true,
      };
    }


    private class BinarySchemaStructure : IBinarySchemaStructure {
      public IList<Diagnostic> Diagnostics { get; set; }
      public INamedTypeSymbol TypeSymbol { get; set; }
      public IReadOnlyList<ISchemaMember> Members { get; set; }
      public Endianness? Endianness { get; set; }
    }

    public class SchemaMethodMember : ISchemaMethodMember {
      public string Name { get; set; }
    }

    public class SchemaValueMember : ISchemaValueMember {
      public string Name { get; set; }
      public IMemberType MemberType { get; set; }
      public bool IsIgnored { get; set; }
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
      public IMemberReference<string>? LengthOfStringMember { get; set; }
      public IMemberReference? LengthOfSequenceMember { get; set; }
      public IChain<IAccessChainNode>? AccessChainToSizeOf { get; set; }
      public IChain<IAccessChainNode>? AccessChainToPointer { get; set; }
    }

    public class StructureMemberType : IStructureMemberType {
      public IStructureTypeInfo StructureTypeInfo { get; set; }
      public ITypeInfo TypeInfo => StructureTypeInfo;
      public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
      public bool IsReadOnly => this.TypeInfo.IsReadOnly;

      public bool IsReferenceType { get; set; }
      public bool IsChild { get; set; }
      public bool IsStruct => StructureTypeInfo.IsStruct;
    }

    public class GenericMemberType : IGenericMemberType {
      public IMemberType ConstraintType { get; set; }
      public IGenericTypeInfo GenericTypeInfo { get; set; }
      public ITypeInfo TypeInfo => GenericTypeInfo;
      public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
      public bool IsReadOnly => this.TypeInfo.IsReadOnly;
    }

    public class Offset : IOffset {
      public ISchemaValueMember OffsetName { get; set; }
    }

    public class StringType : IStringType {
      public ITypeInfo TypeInfo { get; set; }
      public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
      public bool IsReadOnly => this.TypeInfo.IsReadOnly;

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
}