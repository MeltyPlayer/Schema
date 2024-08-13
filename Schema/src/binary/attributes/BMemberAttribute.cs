using System;

using Microsoft.CodeAnalysis;

using schema.binary.parser;
using schema.util.asserts;
using schema.util.diagnostics;
using schema.util.symbols;


namespace schema.binary.attributes;

public abstract class BMemberAttribute<T> : BMemberAttribute {
  protected override void SetMemberFromName(string memberName) {
      this.memberThisIsAttachedTo_ =
          this.GetMemberRelativeToContainer<T>(memberName);
    }
}

public abstract class BMemberAttribute : Attribute {
  private static readonly TypeInfoParser parser_ = new();
  private IDiagnosticReporter? diagnosticReporter_;

  private INamedTypeSymbol containerTypeSymbol_;
  private ITypeInfo containerTypeInfo_;
  protected IMemberReference memberThisIsAttachedTo_;

  protected abstract void InitFields();

  protected virtual void SetMemberFromName(string memberName) {
      this.memberThisIsAttachedTo_ =
          this.GetMemberRelativeToContainer(memberName);
    }


  internal void Init(
      IDiagnosticReporter? diagnosticReporter,
      INamedTypeSymbol containerTypeSymbol,
      string memberName) {
      this.diagnosticReporter_ = diagnosticReporter;
      this.containerTypeSymbol_ = containerTypeSymbol;
      this.containerTypeInfo_ = BMemberAttribute.parser_.AssertParseType(
          containerTypeSymbol);
      this.SetMemberFromName(memberName);
      this.InitFields();
    }


  protected IMemberReference GetMemberRelativeToContainer(
      string memberName) {
      SymbolTypeUtil.GetMemberInContainer(
          this.containerTypeSymbol_,
          memberName,
          out var memberSymbol,
          out var memberTypeSymbol,
          out var memberTypeInfo);

      return new MemberReference(memberName,
                                 this.containerTypeInfo_,
                                 memberSymbol,
                                 memberTypeSymbol,
                                 memberTypeInfo);
    }


  protected IMemberReference<T> GetMemberRelativeToContainer<T>(
      string memberName) {
      SymbolTypeUtil.GetMemberInContainer(
          this.containerTypeSymbol_,
          memberName,
          out var memberSymbol,
          out var memberTypeSymbol,
          out var memberTypeInfo);

      if (!memberTypeInfo.TypeSymbol.Implements<T>()) {
        Asserts.Fail(
            $"Type of member, {memberTypeInfo.TypeSymbol}, does not match expected type: {typeof(T)}");
      }

      return new MemberReference<T>(
          memberName,
          this.containerTypeInfo_,
          memberSymbol,
          memberTypeSymbol,
          memberTypeInfo);
    }

  protected IMemberReference GetOtherMemberRelativeToContainer(
      string otherMemberName) {
      SymbolTypeUtil.GetMemberRelativeToAnother(
          this.diagnosticReporter_,
          this.containerTypeSymbol_,
          otherMemberName,
          this.memberThisIsAttachedTo_.Name,
          true,
          out var memberSymbol,
          out var memberTypeSymbol,
          out var memberTypeInfo);

      return new MemberReference(
          otherMemberName,
          this.containerTypeInfo_,
          memberSymbol,
          memberTypeSymbol,
          memberTypeInfo);
    }

  protected IMemberReference<T> GetOtherMemberRelativeToContainer<T>(
      string otherMemberName) {
      SymbolTypeUtil.GetMemberRelativeToAnother(
          this.diagnosticReporter_,
          this.containerTypeSymbol_,
          otherMemberName,
          this.memberThisIsAttachedTo_.Name,
          true,
          out var memberSymbol,
          out var memberTypeSymbol,
          out var memberTypeInfo);

      if (!memberTypeInfo.TypeSymbol.Implements<T>()) {
        Asserts.Fail(
            $"Type of other member, {memberTypeInfo.TypeSymbol}, does not match expected type: {typeof(T)}");
      }

      return new MemberReference<T>(
          otherMemberName,
          this.containerTypeInfo_,
          memberSymbol,
          memberTypeSymbol,
          memberTypeInfo);
    }


  protected IChain<IAccessChainNode> GetAccessChainRelativeToContainer(
      string otherMemberPath,
      bool assertOrder)
    => AccessChainUtil.GetAccessChainForRelativeMember(
        this.diagnosticReporter_,
        this.containerTypeSymbol_,
        otherMemberPath,
        this.memberThisIsAttachedTo_.Name,
        assertOrder);

  protected IChain<IAccessChainNode> GetAccessChainRelativeToContainer<T>(
      string otherMemberPath,
      bool assertOrder) {
      var typeChain = AccessChainUtil.GetAccessChainForRelativeMember(
          this.diagnosticReporter_,
          this.containerTypeSymbol_,
          otherMemberPath,
          this.memberThisIsAttachedTo_.Name,
          assertOrder);

      var targetTypeSymbol = typeChain.Target.MemberTypeInfo.TypeSymbol;
      if (!targetTypeSymbol.Implements<T>()) {
        Asserts.Fail(
            $"Type of other member, {targetTypeSymbol}, does not match expected type: {typeof(T)}");
      }

      return typeChain;
    }


  protected IMemberReference GetReadTimeOnlySourceRelativeToContainer(
      string otherMemberName) {
      var source = this.GetOtherMemberRelativeToContainer(otherMemberName);

      if (!this.IsMemberWritePrivateOrSkipped_(source.MemberSymbol)) {
        this.diagnosticReporter_.ReportDiagnostic(
            source.MemberSymbol,
            Rules.SourceMustBePrivate);
      }

      return source;
    }

  protected IMemberReference<T> GetReadTimeOnlySourceRelativeToContainer<T>(
      string otherMemberName) {
      var source = this.GetOtherMemberRelativeToContainer<T>(otherMemberName);

      if (!this.IsMemberWritePrivateOrSkipped_(source.MemberSymbol)) {
        this.diagnosticReporter_.ReportDiagnostic(
            source.MemberSymbol,
            Rules.SourceMustBePrivate);
      }

      return source;
    }

  private bool IsMemberWritePrivateOrSkipped_(ISymbol symbol)
    => symbol switch {
           IPropertySymbol propertySymbol
               => (propertySymbol.SetMethod
                                 ?.DeclaredAccessibility ??
                   Accessibility.Private) ==
                  Accessibility.Private,
           IFieldSymbol fieldSymbol
               => fieldSymbol.DeclaredAccessibility == Accessibility.Private,
       } ||
       symbol.HasAttribute<SkipAttribute>();
}


public interface IMemberReference {
  string Name { get; }
  ITypeInfo ContainerTypeInfo { get; }
  ISymbol MemberSymbol { get; }
  ITypeSymbol MemberTypeSymbol { get; }
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
      ITypeInfo containerTypeInfo,
      ISymbol memberSymbol,
      ITypeSymbol memberTypeSymbol,
      ITypeInfo memberTypeInfo) {
      this.Name = name;
      this.ContainerTypeInfo = containerTypeInfo;
      this.MemberSymbol = memberSymbol;
      this.MemberTypeSymbol = memberTypeSymbol;
      this.MemberTypeInfo = memberTypeInfo;
    }

  public string Name { get; }
  public ITypeInfo ContainerTypeInfo { get; }
  public ISymbol MemberSymbol { get; }
  public ITypeSymbol MemberTypeSymbol { get; }
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
      ITypeInfo containerTypeInfo,
      ISymbol memberSymbol,
      ITypeSymbol memberTypeSymbol,
      ITypeInfo memberTypeInfo)
      : base(name,
             containerTypeInfo,
             memberSymbol,
             memberTypeSymbol,
             memberTypeInfo) { }
}