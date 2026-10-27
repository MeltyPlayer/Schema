using System.IO;
using System.Text;

using schema.binary.steps;
using schema.util.symbols;
using schema.util.text;


namespace schema.binary.text;

public class BinarySchemaOffsetsGenerator {
  public const string SCHEMA_SIZE_NAME = "SCHEMA_SIZE";

  public string Generate(IBinarySchemaContainer container) {
    var typeSymbol = container.TypeSymbol;

    var sb = new StringBuilder();
    using var sw = new SourceWriter(new StringWriter(sb));

    sw.WriteNamespaceAndParentTypeBlocks(
        typeSymbol,
        () => {
          sw.WriteLine("/// <remarks>");

          container.ToBinarySteps()
                   .TryToGetMemberOffsetsAndContainerSize(
                       out var memberOffsets,
                       out var containerSize);
          if (containerSize == null) {
            sw.WriteLine("///   Size: (indeterminate)<br/>");
          } else {
            sw.WriteLine($"///   Size: 0x{containerSize.Value:X}<br/>");
          }

          sw.WriteLine("///<br/>");


          if (memberOffsets.Count == 0) {
            sw.WriteLine("///   Offsets: (none)<br/>");
          } else {
            sw.WriteLine("///   Offsets:<br/>");
            foreach (var memberOffset in memberOffsets) {
              sw.WriteLine($"///     - {memberOffset}<br/>");
            }
          }

          sw.WriteLine("/// </remarks>");

          if (containerSize == null) {
            sw.WriteLine(
                $"{typeSymbol.GetQualifiersAndNameAndGenericParametersFor()};");
          } else {
            sw.EnterBlock(
                $"{typeSymbol.GetQualifiersAndNameAndGenericParametersFor()}");
            sw.WriteLine($"public const int {SCHEMA_SIZE_NAME} = 0x{containerSize.Value:X};");
            sw.ExitBlock();
          }
        });

    var generatedCode = sb.ToString();
    return generatedCode;
  }
}