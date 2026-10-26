using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using schema.util.symbols;

namespace schema.binary;

public static class BinarySchemaContainerExtensions {
  public static bool IsRecursive(this IBinarySchemaContainer container) {
    var containerTypeSymbol = container.TypeSymbol;

    var checkedTypeSymbols = new HashSet<ITypeSymbol>();

    var memberQueue = new Queue<ISchemaMember>();
    foreach (var member in container.Members) {
      memberQueue.Enqueue(member);
    }

    while (memberQueue.Count > 0) {
      var member = memberQueue.Dequeue();
      if (member is not ISchemaValueMember valueMember) {
        continue;
      }

      var typeSymbol = valueMember.MemberType.TypeSymbol;
      if (!checkedTypeSymbols.Add(typeSymbol)) {
        continue;
      }

      if (typeSymbol.Equals(containerTypeSymbol)) {
        return true;
      }

      if (valueMember.MemberType is ISequenceMemberType sequenceMemberType) {
        var elementType = sequenceMemberType.ElementType.TypeSymbol;
        if (elementType.Equals(containerTypeSymbol)) {
          return true;
        }

        checkedTypeSymbols.Add(elementType);
      }

      if (valueMember.MemberType is IContainerMemberType containerMemberType) {
        var childContainer = BinarySchemaContainerParser.Instance.ParseContainer(
            BetterSymbol.FromType(
                (containerMemberType.TypeSymbol as INamedTypeSymbol)!));
        foreach (var childMember in childContainer.Members) {
          memberQueue.Enqueue(childMember);
        }
      }
    }

    return false;
  }
}