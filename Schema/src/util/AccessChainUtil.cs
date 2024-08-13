using System;

using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Linq;
using System.Text;

using schema.binary.attributes;
using schema.util.data;
using schema.binary.parser;
using schema.util.asserts;
using schema.util.diagnostics;
using schema.util.symbols;


namespace schema.binary;

public interface IChain<out T> {
  T Root { get; }
  T Target { get; }

  IReadOnlyList<T> RootToTarget { get; }
  string Path { get; }
  string RawPath { get; }
}

public interface IAccessChainNode {
  INamedTypeSymbol ContainerSymbol { get; }
  ISymbol MemberSymbol { get; }
  ITypeSymbol MemberTypeSymbol { get; }
  ITypeInfo MemberTypeInfo { get; }
  bool IsOrderValid { get; }
}


internal static class AccessChainUtil {
  public static IChain<IAccessChainNode> GetAccessChainForRelativeMember(
      IDiagnosticReporter? diagnosticReporter,
      ITypeSymbol containerSymbol,
      string otherMemberPath,
      string thisMemberName,
      bool assertOrder
  ) {
    var typeChain = GetAccessChainForRelativeMemberImpl_(
        diagnosticReporter,
        containerSymbol,
        otherMemberPath,
        thisMemberName,
        new UpDownStack<string>(),
        null,
        null
    );

    if (assertOrder) {
      foreach (var node in typeChain.RootToTarget) {
        if (!node.IsOrderValid &&
            node.MemberSymbol
                .GetAttribute<SkipAttribute>(diagnosticReporter) ==
            null) {
          diagnosticReporter.ReportDiagnostic(
              node.MemberSymbol,
              Rules.DependentMustComeAfterSource);
        }
      }
    }

    return typeChain;
  }

  public static void AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
      IDiagnosticReporter diagnosticReporter,
      IChain<IAccessChainNode> accessChain) {
    for (var i = 0; i < accessChain.RootToTarget.Count; ++i) {
      var typeChainNode = accessChain.RootToTarget[i];

      var binarySchemaAttribute =
          typeChainNode.ContainerSymbol.GetAttribute<BinarySchemaAttribute>(
              diagnosticReporter);
      if (binarySchemaAttribute == null) {
        diagnosticReporter.ReportDiagnostic(
            typeChainNode.MemberSymbol,
            Rules.AllMembersInChainMustUseSchema);
      }
    }
  }

  private static void GetMemberInContainer_(
      ITypeSymbol containerSymbol,
      string memberName,
      out ISymbol memberSymbol,
      out ITypeSymbol memberTypeSymbol,
      out ITypeInfo memberTypeInfo
  ) {
    memberSymbol = containerSymbol.GetMembers(memberName).SingleOrDefault();
    if (memberSymbol == null) {
      throw new Exception(
          $"Expected to find member \"{memberName}\" in container {containerSymbol.Name}");
    }

    new TypeInfoParser().ParseMember(memberSymbol,
                                     out memberTypeSymbol,
                                     out memberTypeInfo);
  }


  private static IChain<IAccessChainNode>
      GetAccessChainForRelativeMemberImpl_(
          IDiagnosticReporter? diagnosticReporter,
          ITypeSymbol containerSymbol,
          string otherMemberPath,
          string thisMemberName,
          IUpDownStack<string> upDownStack,
          AccessChain accessChain,
          string prevMemberName
      ) {
    if (accessChain == null) {
      AccessChainUtil.GetMemberInContainer_(
          containerSymbol,
          thisMemberName,
          out var rootSymbol,
          out var rootTypeSymbol,
          out var rootTypeInfo);

      accessChain = new AccessChain(
          new AccessChainNode {
              ContainerSymbol = (containerSymbol as INamedTypeSymbol)!,
              MemberSymbol = rootSymbol,
              MemberTypeSymbol = rootTypeSymbol,
              MemberTypeInfo = rootTypeInfo,
              IsOrderValid = true,
          },
          otherMemberPath);

      prevMemberName = thisMemberName;
    }

    // Gets next child in chain.
    var periodIndex = otherMemberPath.IndexOf('.');
    var steppingIntoNewContainer = periodIndex != -1;
    var currentMemberName = steppingIntoNewContainer
        ? otherMemberPath.Substring(0, periodIndex)
        : otherMemberPath;

    AccessChainUtil.GetMemberInContainer_(
        containerSymbol,
        currentMemberName,
        out var memberSymbol,
        out var memberTypeSymbol,
        out var memberTypeInfo);

    var comesAfter = true;
    // Asserts that we're not referencing something that comes before the
    // current member.
    if (upDownStack.Count == 0) {
      var members = containerSymbol.GetMembers();
      var membersAndIndices =
          members.Select((member, index) => (member, index)).ToArray();
      var indexOfThisMember = membersAndIndices
                              .Single(memberAndIndex =>
                                          memberAndIndex.member.Name ==
                                          thisMemberName)
                              .index;
      var indexOfOtherMember = membersAndIndices
                               .Single(memberAndIndex =>
                                           memberAndIndex.member.Name ==
                                           currentMemberName)
                               .index;

      if (indexOfThisMember < indexOfOtherMember) {
        comesAfter = false;
      }
    }

    accessChain.AddLinkInChain(new AccessChainNode {
        ContainerSymbol =
            (containerSymbol as INamedTypeSymbol)!,
        MemberSymbol = memberSymbol,
        MemberTypeSymbol = memberTypeSymbol,
        MemberTypeInfo = memberTypeInfo,
        IsOrderValid = comesAfter,
    });

    if (currentMemberName == nameof(IChildOf<IBinaryConvertible>.Parent) &&
        containerSymbol.IsChild(out _)) {
      upDownStack.PushUpFrom(prevMemberName);
    } else {
      upDownStack.PushDownTo(currentMemberName);
    }

    // Steps down into next chain or returns.
    if (steppingIntoNewContainer) {
      var subMemberPath = otherMemberPath.Substring(periodIndex + 1);
      return GetAccessChainForRelativeMemberImpl_(
          diagnosticReporter,
          memberTypeSymbol,
          subMemberPath,
          thisMemberName,
          upDownStack,
          accessChain,
          currentMemberName);
    }

    // Asserts that we didn't ultimately reach the same member as this.
    if (upDownStack.Count == 0 &&
        currentMemberName == thisMemberName) {
      Asserts.Fail(
          $"Expected to find '{currentMemberName}' relative to '{thisMemberName}' in '{containerSymbol.Name}', but they're the same!");
    }

    // Gathers the path to the node
    var totalPath = new StringBuilder();
    foreach (var node in upDownStack.Reverse()) {
      if (node is IDownStackNode<string> downStackNode) {
        if (totalPath.Length > 0) {
          totalPath.Append(".");
        }

        totalPath.Append(downStackNode.ToValue);
      }
    }

    accessChain.Path = totalPath.ToString();

    return accessChain;
  }

  private class AccessChain : IChain<IAccessChainNode> {
    private readonly List<IAccessChainNode> rootToTarget_ = new();

    public AccessChain(IAccessChainNode root, string rawPath) {
      this.AddLinkInChain(root);
      this.RawPath = rawPath;
    }

    public IAccessChainNode Root => this.RootToTarget.First();
    public IAccessChainNode Target => this.RootToTarget.Last();
    public IReadOnlyList<IAccessChainNode> RootToTarget => this.rootToTarget_;

    public void AddLinkInChain(IAccessChainNode node)
      => this.rootToTarget_.Add(node);

    public string Path { get; set; }
    public string RawPath { get; }
  }

  private class AccessChainNode : IAccessChainNode {
    public INamedTypeSymbol ContainerSymbol { get; set; }
    public ISymbol MemberSymbol { get; set; }
    public ITypeSymbol MemberTypeSymbol { get; set; }
    public ITypeInfo MemberTypeInfo { get; set; }
    public bool IsOrderValid { get; set; }
  }
}