using System.IO;
using System.Text;

using schema.binary.steps;
using schema.util.symbols;
using schema.util.text;


namespace schema.binary.text;

public class BinarySchemaOffsetsGenerator {
  public string Generate(IBinarySchemaContainer container) {
    var typeSymbol = container.TypeSymbol;

    var sb = new StringBuilder();
    using var sw = new SourceWriter(new StringWriter(sb));

    sw.WriteNamespaceAndParentTypeBlocks(
        typeSymbol,
        () => {
          sw.WriteLine("/// <content>");

          container.ToBinarySteps()
                   .TryToGetMemberOffsetsAndContainerSize(
                       out var knownMemberOffsets,
                       out var containerSize);
          if (containerSize == null) {
            sw.WriteLine("///   Size: indeterminate");
          } else {
            sw.WriteLine($"///   Size: 0x{containerSize.Value:X} / {containerSize.Value}");
          }

          sw.WriteLine(" /// ");

          sw.WriteLine(" ///   Determinate offsets:");
          foreach (var (member, offset) in knownMemberOffsets) {
            sw.WriteLine($" ///     - {member.Name}: 0x{offset:X} / {offset}");
          }

          sw.WriteLine("/// </content>");

          sw.WriteLine(
              $"{typeSymbol.GetQualifiersAndNameAndGenericParametersFor()};");
        });

    var generatedCode = sb.ToString();
    return generatedCode;
  }
}