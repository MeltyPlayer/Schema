using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Linq;
using System.Text;

using schema.binary.attributes.child_of;
using schema.binary.io;
using schema.binary.parser;
using schema.binary.util;


namespace schema.binary {
  public interface IChain<out T> {
    T Root { get; }
    T Target { get; }

    IReadOnlyList<T> RootToTarget { get; }
    string Path { get; }
  }

  public interface IAccessChainNode {
    INamedTypeSymbol StructureSymbol { get; }
    ISymbol MemberSymbol { get; }
    ITypeInfo MemberTypeInfo { get; }
  }


  internal static class AccessChainUtil {
    public static IChain<IAccessChainNode> GetAccessChainForRelativeMember(
        IList<Diagnostic> diagnostics,
        ITypeSymbol structureSymbol,
        string otherMemberPath,
        string thisMemberName,
        bool assertOrder
    )
      => GetAccessChainForRelativeMemberImpl_(
          diagnostics,
          structureSymbol,
          otherMemberPath,
          thisMemberName,
          assertOrder,
          new UpDownStack<string>(),
          null,
          null
      );

    public static void AssertAllNodesInTypeChainUntilTargetUseBinarySchema(
        IList<Diagnostic> diagnostics,
        IChain<IAccessChainNode> accessChain) {
      for (var i = 0; i < accessChain.RootToTarget.Count; ++i) {
        var typeChainNode = accessChain.RootToTarget[i];

        var binarySchemaAttribute =
            SymbolTypeUtil.GetAttribute<BinarySchemaAttribute>(
                diagnostics, typeChainNode.StructureSymbol);
        if (binarySchemaAttribute == null) {
          diagnostics.Add(Rules.CreateDiagnostic(
                              typeChainNode.MemberSymbol,
                              Rules.AllMembersInChainMustUseSchema));
        }
      }
    }

    private static void GetMemberInStructure_(
        ITypeSymbol structureSymbol,
        string memberName,
        out ISymbol memberSymbol,
        out ITypeInfo memberTypeInfo
    ) {
      memberSymbol = structureSymbol.GetMembers(memberName).Single();
      new TypeInfoParser().ParseMember(memberSymbol, out memberTypeInfo);
    }


    private static IChain<IAccessChainNode>
        GetAccessChainForRelativeMemberImpl_(
            IList<Diagnostic> diagnostics,
            ITypeSymbol structureSymbol,
            string otherMemberPath,
            string thisMemberName,
            bool assertOrder,
            IUpDownStack<string> upDownStack,
            AccessChain accessChain,
            string prevMemberName
        ) {
      if (accessChain == null) {
        GetMemberInStructure_(
            structureSymbol,
            thisMemberName,
            out var rootSymbol,
            out var rootTypeInfo);

        accessChain = new AccessChain(new AccessChainNode {
            StructureSymbol = (structureSymbol as INamedTypeSymbol)!,
            MemberSymbol = rootSymbol,
            MemberTypeInfo = rootTypeInfo
        });

        prevMemberName = thisMemberName;
      }

      // Gets next child in chain.
      var periodIndex = otherMemberPath.IndexOf('.');
      var steppingIntoNewStructure = periodIndex != -1;
      var currentMemberName = steppingIntoNewStructure
                                  ? otherMemberPath.Substring(0, periodIndex)
                                  : otherMemberPath;

      GetMemberInStructure_(
          structureSymbol,
          currentMemberName,
          out var memberSymbol,
          out var memberTypeInfo);

      accessChain.AddLinkInChain(new AccessChainNode {
          StructureSymbol = (structureSymbol as INamedTypeSymbol)!,
          MemberSymbol = memberSymbol,
          MemberTypeInfo = memberTypeInfo
      });

      // Asserts that we're not referencing something that comes before the
      // current member.
      if (assertOrder && upDownStack.Count == 0) {
        var members = structureSymbol.GetMembers();
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
          diagnostics.Add(Rules.CreateDiagnostic(
                              members[indexOfThisMember],
                              Rules.DependentMustComeAfterSource));
        }
      }

      if (currentMemberName == nameof(IChildOf<IBinaryConvertible>.Parent) &&
          new ChildOfParser(diagnostics).GetParentTypeSymbolOf(
              (structureSymbol as INamedTypeSymbol)!) != null) {
        upDownStack.PushUpFrom(prevMemberName);
      } else {
        upDownStack.PushDownTo(currentMemberName);
      }

      // Steps down into next chain or returns.
      if (steppingIntoNewStructure) {
        var subMemberPath = otherMemberPath.Substring(periodIndex + 1);
        return GetAccessChainForRelativeMemberImpl_(
            diagnostics,
            (memberTypeInfo.TypeSymbol as INamedTypeSymbol)!,
            subMemberPath,
            thisMemberName,
            assertOrder,
            upDownStack,
            accessChain,
            currentMemberName);
      }

      // Asserts that we didn't ultimately reach the same member as this.
      if (upDownStack.Count == 0 &&
          currentMemberName == thisMemberName) {
        Asserts.Fail(
            $"Expected to find '{currentMemberName}' relative to '{thisMemberName}' in '{structureSymbol.Name}', but they're the same!");
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

      public AccessChain(IAccessChainNode root) => this.AddLinkInChain(root);

      public IAccessChainNode Root => this.RootToTarget.First();
      public IAccessChainNode Target => this.RootToTarget.Last();
      public IReadOnlyList<IAccessChainNode> RootToTarget => this.rootToTarget_;

      public void AddLinkInChain(IAccessChainNode node)
        => this.rootToTarget_.Add(node);

      public string Path { get; set; }
    }

    private class AccessChainNode : IAccessChainNode {
      public INamedTypeSymbol StructureSymbol { get; set; }
      public ISymbol MemberSymbol { get; set; }
      public ITypeInfo MemberTypeInfo { get; set; }
    }
  }
}