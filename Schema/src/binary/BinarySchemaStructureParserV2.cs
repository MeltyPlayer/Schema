using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using schema.binary.parser;
using schema.util.symbols;


namespace schema.binary;

internal interface IBinarySchemaContainerParserV2 {
  IBinarySchemaContainerV2 ParseContainer(
      IBetterSymbol<INamedTypeSymbol> containerSymbol);
}

internal interface IBinarySchemaContainerV2 {
  IBetterSymbol<INamedTypeSymbol> TypeSymbol { get; }
  IReadOnlyList<IBinarySchemaMemberV2> Members { get; }
}

internal interface IBinarySchemaMemberV2 {
  IBetterSymbol MemberSymbol { get; }
  ITypeInfo TypeInfo { get; }
}

internal class BinarySchemaContainerParserV2 : IBinarySchemaContainerParserV2 {
  public IBinarySchemaContainerV2 ParseContainer(
      IBetterSymbol<INamedTypeSymbol> containerSymbol) {
    var typeInfoParser = new TypeInfoParser();
    var parsedMembers =
        typeInfoParser.ParseMembers(containerSymbol.TypedSymbol).ToArray();

    var members = new List<IBinarySchemaMemberV2>();
    foreach (var (status, memberSymbol, _, typeInfo) in parsedMembers) {
      if (status is not TypeInfoParser.ParseStatus.SUCCESS) {
        continue;
      }

      members.Add(new BinarySchemaMemberV2Impl(containerSymbol.GetMember(memberSymbol), typeInfo!));
    }

    return new BinarySchemaContainerV2Impl(containerSymbol, members);
  }

  private record BinarySchemaContainerV2Impl(
      IBetterSymbol<INamedTypeSymbol> TypeSymbol,
      IReadOnlyList<IBinarySchemaMemberV2> Members) : IBinarySchemaContainerV2;

  private record BinarySchemaMemberV2Impl(
      IBetterSymbol MemberSymbol,
      ITypeInfo TypeInfo) : IBinarySchemaMemberV2;
}