using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.Linq;


namespace schema.binary.attributes;

public class WSizeOfMemberInBytesDependencyFixer {
  public void AddDependenciesForContainer(
      IReadOnlyDictionary<INamedTypeSymbol, IBinarySchemaContainer>
          containerByNamedTypeSymbol,
      IChain<IAccessChainNode> accessChain) {
    foreach (var typeChainNode in accessChain.RootToTarget.Skip(1)) {
      if (containerByNamedTypeSymbol.TryGetValue(
              typeChainNode.ContainerSymbol,
              out var container)) {
        var member = container.Members.Single(
                member =>
                    member.Name ==
                    typeChainNode.MemberSymbol.Name)
            as BinarySchemaContainerParser.SchemaValueMember;
        member.TrackStartAndEnd = true;
      }
    }
  }
}