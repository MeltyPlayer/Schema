using System;

using Microsoft.CodeAnalysis;

using schema.binary.parser;
using schema.util;
using schema.util.diagnostics;
using schema.util.symbols;

namespace schema.binary.attributes {
  public abstract class BMemberAttribute<T> : BMemberAttribute {
    protected override void SetMemberFromName(string memberName) {
      this.memberThisIsAttachedTo_ =
          this.GetMemberRelativeToStructure<T>(memberName);
    }
  }

  public abstract class BMemberAttribute : Attribute {
    private static readonly TypeInfoParser parser_ = new();
    private IDiagnosticReporter diagnosticReporter_;

    private ITypeInfo structureTypeInfo_;
    protected IMemberReference memberThisIsAttachedTo_;

    protected abstract void InitFields();

    protected virtual void SetMemberFromName(string memberName) {
      this.memberThisIsAttachedTo_ =
          this.GetMemberRelativeToStructure(memberName);
    }


    internal void Init(
        IDiagnosticReporter diagnosticReporter,
        ITypeSymbol structureTypeSymbol,
        string memberName) {
      this.diagnosticReporter_ = diagnosticReporter;
      this.structureTypeInfo_ = BMemberAttribute.parser_.AssertParseTypeSymbol(
          structureTypeSymbol);
      this.SetMemberFromName(memberName);
      this.InitFields();
    }


    protected IMemberReference GetMemberRelativeToStructure(
        string memberName) {
      SymbolTypeUtil.GetMemberInStructure(
          this.structureTypeInfo_.TypeSymbol,
          memberName,
          out var memberSymbol,
          out var memberTypeInfo);

      return new MemberReference(memberName,
                                 this.structureTypeInfo_,
                                 memberSymbol,
                                 memberTypeInfo);
    }


    protected IMemberReference<T> GetMemberRelativeToStructure<T>(
        string memberName) {
      SymbolTypeUtil.GetMemberInStructure(
          this.structureTypeInfo_.TypeSymbol,
          memberName,
          out var memberSymbol,
          out var memberTypeInfo);

      if (!SymbolTypeUtil.CanBeStoredAs(memberTypeInfo.TypeSymbol, typeof(T))) {
        Asserts.Fail(
            $"Type of member, {memberTypeInfo.TypeSymbol}, does not match expected type: {typeof(T)}");
      }

      return new MemberReference<T>(
          memberName,
          this.structureTypeInfo_,
          memberSymbol,
          memberTypeInfo);
    }

    protected IMemberReference GetOtherMemberRelativeToStructure(
        string otherMemberName) {
      SymbolTypeUtil.GetMemberRelativeToAnother(
          this.diagnosticReporter_,
          this.structureTypeInfo_.TypeSymbol,
          otherMemberName,
          this.memberThisIsAttachedTo_.Name,
          true,
          out var memberSymbol,
          out var memberTypeInfo);

      return new MemberReference(
          otherMemberName,
          this.structureTypeInfo_,
          memberSymbol,
          memberTypeInfo);
    }

    protected IMemberReference<T> GetOtherMemberRelativeToStructure<T>(
        string otherMemberName) {
      SymbolTypeUtil.GetMemberRelativeToAnother(
          this.diagnosticReporter_,
          this.structureTypeInfo_.TypeSymbol,
          otherMemberName,
          this.memberThisIsAttachedTo_.Name,
          true,
          out var memberSymbol,
          out var memberTypeInfo);

      if (!SymbolTypeUtil.CanBeStoredAs(memberTypeInfo.TypeSymbol, typeof(T))) {
        Asserts.Fail(
            $"Type of other member, {memberTypeInfo.TypeSymbol}, does not match expected type: {typeof(T)}");
      }

      return new MemberReference<T>(
          otherMemberName,
          this.structureTypeInfo_,
          memberSymbol,
          memberTypeInfo);
    }


    protected IChain<IAccessChainNode> GetAccessChainRelativeToStructure(
        string otherMemberPath,
        bool assertOrder)
      => AccessChainUtil.GetAccessChainForRelativeMember(
          this.diagnosticReporter_,
          this.structureTypeInfo_.TypeSymbol,
          otherMemberPath,
          this.memberThisIsAttachedTo_.Name,
          assertOrder);

    protected IChain<IAccessChainNode> GetAccessChainRelativeToStructure<T>(
        string otherMemberPath,
        bool assertOrder) {
      var typeChain = AccessChainUtil.GetAccessChainForRelativeMember(
          this.diagnosticReporter_,
          this.structureTypeInfo_.TypeSymbol,
          otherMemberPath,
          this.memberThisIsAttachedTo_.Name,
          assertOrder);

      var targetTypeSymbol = typeChain.Target.MemberTypeInfo.TypeSymbol;
      if (!SymbolTypeUtil.CanBeStoredAs(targetTypeSymbol, typeof(T))) {
        Asserts.Fail(
            $"Type of other member, {targetTypeSymbol}, does not match expected type: {typeof(T)}");
      }

      return typeChain;
    }


    protected IMemberReference GetReadTimeOnlySourceRelativeToStructure(
        string otherMemberName) {
      var source = this.GetOtherMemberRelativeToStructure(otherMemberName);

      if (!IsMemberWritePrivateOrIgnored_(source.MemberSymbol)) {
        this.diagnosticReporter_.ReportDiagnostic(
            source.MemberSymbol,
            Rules.SourceMustBePrivate);
      }

      return source;
    }

    protected IMemberReference<T> GetReadTimeOnlySourceRelativeToStructure<T>(
        string otherMemberName) {
      var source = this.GetOtherMemberRelativeToStructure<T>(otherMemberName);

      if (!IsMemberWritePrivateOrIgnored_(source.MemberSymbol)) {
        this.diagnosticReporter_.ReportDiagnostic(
            source.MemberSymbol,
            Rules.SourceMustBePrivate);
      }

      return source;
    }

    private bool IsMemberWritePrivateOrIgnored_(ISymbol symbol)
      => symbol switch {
          IPropertySymbol propertySymbol
              => (propertySymbol.SetMethod
                                ?.DeclaredAccessibility ??
                  Accessibility.Private) == Accessibility.Private,
          IFieldSymbol fieldSymbol
              => fieldSymbol.DeclaredAccessibility == Accessibility.Private,
      } || symbol.HasAttribute<IgnoreAttribute>();
  }


  public interface IMemberReference {
    string Name { get; }
    ITypeInfo StructureTypeInfo { get; }
    ISymbol MemberSymbol { get; }
    ITypeInfo MemberTypeInfo { get; }

    bool IsInteger { get; }
    IMemberReference AssertIsInteger();

    bool IsBool { get; }
    IMemberReference AssertIsBool();

    bool IsSequence { get; }
    IMemberReference AssertIsSequence();
  }

  public interface IMemberReference<T> : IMemberReference { }


  public class MemberReference : IMemberReference {
    public MemberReference(
        string name,
        ITypeInfo structureTypeInfo,
        ISymbol memberSymbol,
        ITypeInfo memberTypeInfo) {
      this.Name = name;
      this.StructureTypeInfo = structureTypeInfo;
      this.MemberSymbol = memberSymbol;
      this.MemberTypeInfo = memberTypeInfo;
    }

    public string Name { get; }
    public ITypeInfo StructureTypeInfo { get; }
    public ISymbol MemberSymbol { get; }
    public ITypeInfo MemberTypeInfo { get; }

    public bool IsInteger => this.MemberTypeInfo is IIntegerTypeInfo;

    public IMemberReference AssertIsInteger() {
      if (!this.IsInteger) {
        Asserts.Fail($"Expected {this.Name} to refer to an integer!");
      }

      return this;
    }

    public bool IsBool => this.MemberTypeInfo is IBoolTypeInfo;

    public IMemberReference AssertIsBool() {
      if (!this.IsBool) {
        Asserts.Fail($"Expected {this.Name} to refer to an bool!");
      }

      return this;
    }

    public bool IsSequence => this.MemberTypeInfo is ISequenceTypeInfo;

    public IMemberReference AssertIsSequence() {
      if (!this.IsSequence) {
        Asserts.Fail($"Expected {this.Name} to refer to a sequence!");
      }

      return this;
    }
  }

  public class MemberReference<T> : MemberReference, IMemberReference<T> {
    public MemberReference(
        string name,
        ITypeInfo structureTypeInfo,
        ISymbol memberSymbol,
        ITypeInfo memberTypeInfo)
        : base(name,
               structureTypeInfo,
               memberSymbol,
               memberTypeInfo) { }
  }
}