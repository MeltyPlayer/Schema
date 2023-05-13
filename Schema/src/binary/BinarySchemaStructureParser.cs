using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

using System.IO;

using schema.binary.attributes.align;
using schema.binary.attributes.child_of;
using schema.binary.attributes.endianness;
using schema.binary.attributes.ignore;
using schema.binary.attributes.memory;
using schema.binary.attributes.offset;
using schema.binary.attributes.position;
using schema.binary.attributes.sequence;
using schema.binary.attributes.size;
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
    IMemberType MemberType { get; }
    bool IsIgnored { get; }
    int Align { get; }
    IIfBoolean? IfBoolean { get; }
    IOffset? Offset { get; }
    bool IsPosition { get; }
    Endianness? Endianness { get; }
    bool TrackStartAndEnd { get; }
  }

  public interface IMemberType {
    ITypeInfo TypeInfo { get; }
    ITypeSymbol TypeSymbol { get; }
    bool IsReadonly { get; }
  }

  public interface IPrimitiveMemberType : IMemberType {
    SchemaPrimitiveType PrimitiveType { get; }
    bool UseAltFormat { get; }
    SchemaNumberType AltFormat { get; }

    bool SizeOfStream { get; }
    IChain<IAccessChainNode>? AccessChainToSizeOf { get; }
    IChain<IAccessChainNode>? AccessChainToPointer { get; }
  }

  public interface IStructureMemberType : IMemberType {
    IStructureTypeInfo StructureTypeInfo { get; }
    bool IsReferenceType { get; }
    bool IsChild { get; }
  }

  public interface IGenericMemberType : IMemberType {
    IMemberType ConstraintType { get; }
    IGenericTypeInfo GenericTypeInfo { get; }
  }

  public enum IfBooleanSourceType {
    UNSPECIFIED,
    IMMEDIATE_VALUE,
    OTHER_MEMBER,
  }

  public interface IIfBoolean {
    IfBooleanSourceType SourceType { get; }

    SchemaIntegerType ImmediateBooleanType { get; }
    ISchemaMember? BooleanMember { get; }
  }

  public interface IOffset {
    ISchemaMember StartIndexName { get; }
    ISchemaMember OffsetName { get; }
  }

  public enum StringLengthSourceType {
    UNSPECIFIED,
    IMMEDIATE_VALUE,
    OTHER_MEMBER,
    CONST,
    NULL_TERMINATED,
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

    SchemaIntegerType ImmediateLengthType { get; }
    ISchemaMember? LengthMember { get; }
    int ConstLength { get; }
  }

  public enum SequenceType {
    ARRAY,
    LIST,
  }

  public enum SequenceLengthSourceType {
    UNSPECIFIED,
    IMMEDIATE_VALUE,
    OTHER_MEMBER,
    CONST_LENGTH,
    READONLY,
    UNTIL_END_OF_STREAM,
  }

  public interface ISequenceMemberType : IMemberType {
    ISequenceTypeInfo SequenceTypeInfo { get; }

    SequenceType SequenceType { get; }

    SequenceLengthSourceType LengthSourceType { get; }
    SchemaIntegerType ImmediateLengthType { get; }
    ISchemaMember? LengthMember { get; }
    uint ConstLength { get; }

    IMemberType ElementType { get; }
  }

  public class BinarySchemaStructureParser : IBinarySchemaStructureParser {
    public IBinarySchemaStructure ParseStructure(INamedTypeSymbol structureSymbol) {
      var diagnostics = new List<Diagnostic>();

      // All of the types that contain the structure need to be partial
      new PartialContainerAsserter(diagnostics).AssertContainersArePartial(
          structureSymbol);

      var iChildOfParser = new ChildOfParser(diagnostics);
      var parentTypeSymbol =
          iChildOfParser.GetParentTypeSymbolOf(structureSymbol);
      if (parentTypeSymbol != null) {
        iChildOfParser.AssertParentContainsChild(
            parentTypeSymbol, structureSymbol);
      }

      var structureEndianness =
          new EndiannessParser().GetEndianness(diagnostics, structureSymbol);

      var typeInfoParser = new TypeInfoParser();
      var parsedMembers =
          typeInfoParser.ParseMembers(structureSymbol).ToArray();

      var fields = new List<ISchemaMember>();
      foreach (var parsedMember in parsedMembers) {
        var (parseStatus, memberSymbol, _) = parsedMember;
        if (parseStatus == TypeInfoParser.ParseStatus.NOT_A_FIELD_OR_PROPERTY) {
          continue;
        }

        var isIgnored = false;
        if (SymbolTypeUtil.GetAttribute<IgnoreAttribute>(
                diagnostics, memberSymbol) != null) {
          isIgnored = true;
        }

        // Skips parent field for child types
        if (memberSymbol.Name == nameof(IChildOf<IBinaryConvertible>.Parent)
            && parentTypeSymbol != null) {
          isIgnored = true;
        }

        var field =
            !isIgnored
                ? this.ParseNonIgnoredField_(
                    diagnostics,
                    structureSymbol,
                    parsedMember)
                : this.ParseIgnoredField_(parsedMember);

        if (field != null) {
          fields.Add(field);
        }
      }

      var schemaStructure = new BinarySchemaStructure {
          Diagnostics = diagnostics,
          TypeSymbol = structureSymbol,
          Members = fields,
          Endianness = structureEndianness,
      };

      // Hooks up size of dependencies.
      var structureByNamedTypeSymbol =
          new Dictionary<INamedTypeSymbol, IBinarySchemaStructure>();
      structureByNamedTypeSymbol[structureSymbol] = schemaStructure;
      {
        var sizeOfMemberInBytesDependencyFixer =
            new WSizeOfMemberInBytesDependencyFixer();
        foreach (var member in fields) {
          if (member.MemberType is IPrimitiveMemberType primitiveMemberType) {
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

    private ISchemaMember? ParseNonIgnoredField_(
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

      var typeOfSerializable = SymbolTypeUtil.Implements(
              structureSymbol,
              typeof(IBinaryConvertible)) ? typeof(IBinaryConvertible) :
          SymbolTypeUtil.Implements(
              structureSymbol,
              typeof(IBinaryDeserializable)) ? typeof(IBinaryDeserializable) :
          typeof(IBinarySerializable);

      // Makes sure the member is serializable
      {
        if (memberTypeInfo is IStructureTypeInfo structureTypeInfo) {
          if (!SymbolTypeUtil.Implements(
                  structureTypeInfo.NamedTypeSymbol,
                  typeOfSerializable)) {
            diagnostics.Add(
                Rules.CreateDiagnostic(
                    memberSymbol,
                    Rules.StructureMemberNeedsToImplementIBiSerializable));
            return null;
          }
        }
      }

      var memberEndianness =
          new EndiannessParser().GetEndianness(diagnostics, memberSymbol);

      // Gets the type of the current member
      var memberType =
          MemberReferenceUtil.WrapTypeInfoWithMemberType(memberTypeInfo);

      // Get attributes
      var align = new AlignAttributeParser().GetAlignForMember(
          diagnostics, memberSymbol);

      new WSizeOfMemberInBytesParser().Parse(diagnostics, memberSymbol,
                                            memberTypeInfo, memberType);
      new WPointerToParser().Parse(diagnostics, memberSymbol, memberTypeInfo,
                                  memberType);
      {
        var sizeOfStreamAttribute =
            SymbolTypeUtil.GetAttribute<WSizeOfStreamInBytesAttribute>(
                diagnostics, memberSymbol);
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
            SymbolTypeUtil.GetAttribute<PositionRelativeToStreamAttribute>(
                diagnostics, memberSymbol);
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

      IIfBoolean? ifBoolean = null;
      {
        var ifBooleanAttribute =
            (IIfBooleanAttribute?) SymbolTypeUtil
                .GetAttribute<IfBooleanAttribute>(
                    diagnostics,
                    memberSymbol) ??
            SymbolTypeUtil.GetAttribute<RIfBooleanAttribute>(
                diagnostics,
                memberSymbol);
        if (ifBooleanAttribute != null) {
          if (memberTypeInfo.IsNullable) {
            SchemaMember? booleanMember = null;
            if (ifBooleanAttribute.Method ==
                IfBooleanSourceType.OTHER_MEMBER) {
              booleanMember =
                  MemberReferenceUtil.WrapMemberReference(
                      ifBooleanAttribute.OtherMember!);
            }

            ifBoolean = new IfBoolean {
                SourceType = ifBooleanAttribute.Method,
                ImmediateBooleanType = ifBooleanAttribute.BooleanType,
                BooleanMember = booleanMember,
            };
          } else {
            diagnostics.Add(
                Rules.CreateDiagnostic(memberSymbol,
                                       Rules
                                           .IfBooleanNeedsNullable));
          }
        }
      }

      IOffset? offset = null;
      {
        var offsetAttribute =
            SymbolTypeUtil.GetAttribute<OffsetAttribute>(
                diagnostics, memberSymbol);

        if (offsetAttribute != null) {
          var startIndexName = offsetAttribute.StartIndexName;
          SymbolTypeUtil.GetMemberRelativeToAnother(
              diagnostics,
              structureSymbol,
              startIndexName,
              memberSymbol.Name,
              true,
              out _,
              out var startIndexTypeInfo);

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
              StartIndexName = new SchemaMember {
                  Name = startIndexName,
                  MemberType =
                      MemberReferenceUtil.WrapTypeInfoWithMemberType(
                          startIndexTypeInfo),
              },
              OffsetName = new SchemaMember {
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
                      diagnostics, memberSymbol);
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
                      diagnostics, memberSymbol);
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
        var nullTerminatedStringAttribute =
            SymbolTypeUtil.GetAttribute<NullTerminatedStringAttribute>(
                diagnostics, memberSymbol);

        if (stringLengthSourceAttribute != null ||
            nullTerminatedStringAttribute != null) {
          if (memberType is StringType stringType) {
            if (stringLengthSourceAttribute != null &&
                nullTerminatedStringAttribute != null) {
              diagnostics.Add(
                  Rules.CreateDiagnostic(memberSymbol,
                                         Rules.NotSupported));
            }

            if (memberTypeInfo.IsReadonly) {
              diagnostics.Add(
                  Rules.CreateDiagnostic(memberSymbol,
                                         Rules.UnexpectedAttribute));
            }

            var method =
                stringLengthSourceAttribute?.Method ??
                StringLengthSourceType.NULL_TERMINATED;

            switch (method) {
              case StringLengthSourceType.CONST: {
                stringType.LengthSourceType = StringLengthSourceType.CONST;
                stringType.ConstLength =
                    stringLengthSourceAttribute!.ConstLength;
                break;
              }
              case StringLengthSourceType.NULL_TERMINATED: {
                stringType.LengthSourceType =
                    StringLengthSourceType.NULL_TERMINATED;
                break;
              }
              case StringLengthSourceType.IMMEDIATE_VALUE: {
                stringType.LengthSourceType =
                    StringLengthSourceType.IMMEDIATE_VALUE;
                stringType.ImmediateLengthType =
                    stringLengthSourceAttribute.LengthType;
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

      new SequenceLengthSourceParser().Parse(
          diagnostics,
          memberSymbol,
          memberType);

      return new SchemaMember {
          Name = memberSymbol.Name,
          MemberType = memberType,
          IsIgnored = false,
          Align = align,
          IfBoolean = ifBoolean,
          Offset = offset,
          IsPosition = isPosition,
          Endianness = memberEndianness,
      };
    }

    private ISchemaMember ParseIgnoredField_(
        (TypeInfoParser.ParseStatus, ISymbol, ITypeInfo) parsedMember
    ) {
      var (_, memberSymbol, memberTypeInfo) = parsedMember;

      // Gets the type of the current member
      var memberType =
          MemberReferenceUtil.WrapTypeInfoWithMemberType(memberTypeInfo);

      return new SchemaMember {
          Name = memberSymbol.Name, MemberType = memberType, IsIgnored = true,
      };
    }


    private class BinarySchemaStructure : IBinarySchemaStructure {
      public IList<Diagnostic> Diagnostics { get; set; }
      public INamedTypeSymbol TypeSymbol { get; set; }
      public IReadOnlyList<ISchemaMember> Members { get; set; }
      public Endianness? Endianness { get; set; }
    }


    public class SchemaMember : ISchemaMember {
      public string Name { get; set; }
      public IMemberType MemberType { get; set; }
      public bool IsIgnored { get; set; }
      public int Align { get; set; }
      public IIfBoolean IfBoolean { get; set; }
      public IOffset Offset { get; set; }
      public bool IsPosition { get; set; }
      public Endianness? Endianness { get; set; }
      public bool TrackStartAndEnd { get; set; }
    }

    public class PrimitiveMemberType : IPrimitiveMemberType {
      public IPrimitiveTypeInfo PrimitiveTypeInfo { get; set; }
      public ITypeInfo TypeInfo => PrimitiveTypeInfo;
      public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
      public bool IsReadonly => this.TypeInfo.IsReadonly;

      public SchemaPrimitiveType PrimitiveType
        => this.PrimitiveTypeInfo.PrimitiveType;

      public bool UseAltFormat { get; set; }
      public SchemaNumberType AltFormat { get; set; }

      public bool SizeOfStream { get; set; }
      public IChain<IAccessChainNode>? AccessChainToSizeOf { get; set; }
      public IChain<IAccessChainNode>? AccessChainToPointer { get; set; }
    }

    public class StructureMemberType : IStructureMemberType {
      public IStructureTypeInfo StructureTypeInfo { get; set; }
      public ITypeInfo TypeInfo => StructureTypeInfo;
      public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
      public bool IsReadonly => this.TypeInfo.IsReadonly;

      public bool IsReferenceType { get; set; }
      public bool IsChild { get; set; }
    }

    public class GenericMemberType : IGenericMemberType {
      public IMemberType ConstraintType { get; set; }
      public IGenericTypeInfo GenericTypeInfo { get; set; }
      public ITypeInfo TypeInfo => GenericTypeInfo;
      public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
      public bool IsReadonly => this.TypeInfo.IsReadonly;
    }

    public class IfBoolean : IIfBoolean {
      public IfBooleanSourceType SourceType { get; set; }
      public SchemaIntegerType ImmediateBooleanType { get; set; }
      public ISchemaMember? BooleanMember { get; set; }
    }

    public class Offset : IOffset {
      public ISchemaMember StartIndexName { get; set; }
      public ISchemaMember OffsetName { get; set; }
    }

    public class StringType : IStringType {
      public ITypeInfo TypeInfo { get; set; }
      public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
      public bool IsReadonly => this.TypeInfo.IsReadonly;

      public StringLengthSourceType LengthSourceType { get; set; }
      public SchemaIntegerType ImmediateLengthType { get; set; }
      public ISchemaMember? LengthMember { get; set; }
      public int ConstLength { get; set; }
    }

    public class SequenceMemberType : ISequenceMemberType {
      public ISequenceTypeInfo SequenceTypeInfo { get; set; }
      public ITypeInfo TypeInfo => SequenceTypeInfo;
      public ITypeSymbol TypeSymbol => TypeInfo.TypeSymbol;
      public bool IsReadonly => this.TypeInfo.IsReadonly;

      public SequenceType SequenceType { get; set; }

      public SequenceLengthSourceType LengthSourceType { get; set; }
      public SchemaIntegerType ImmediateLengthType { get; set; }
      public ISchemaMember? LengthMember { get; set; }
      public uint ConstLength { get; set; }

      public IMemberType ElementType { get; set; }
    }
  }
}