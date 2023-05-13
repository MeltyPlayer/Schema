using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;


namespace schema.binary.attributes.size {
  public class WSizeOfMemberInBytesDependencyFixer {
    public void AddDependenciesForStructure(
        IDictionary<INamedTypeSymbol, IBinarySchemaStructure>
            structureByNamedTypeSymbol,
        IChain<IAccessChainNode> accessChain) {
      foreach (var typeChainNode in accessChain.RootToTarget.Skip(1)) {
        if (structureByNamedTypeSymbol.TryGetValue(
                typeChainNode.StructureSymbol, out var structure)) {
          var member = structure.Members.Single(
                               member =>
                                   member.Name ==
                                   typeChainNode.MemberSymbol.Name)
                           as BinarySchemaStructureParser.SchemaMember;
          member.TrackStartAndEnd = true;
        }
      }
    }
  }
}