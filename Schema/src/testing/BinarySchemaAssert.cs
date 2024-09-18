using System.IO;
using System.Threading.Tasks;

using schema.util.asserts;


namespace schema.binary.testing;

public static class BinarySchemaAssert {
  public static async Task<byte[]> GetEndianBinaryWriterBytes(
      SchemaBinaryWriter bw) {
    var outputStream = new MemoryStream();
    await bw.CompleteAndCopyToAsync(outputStream);
    return outputStream.ToArray();
  }

  public static async Task WritesAndReadsIdentically<T>(
      T value,
      Endianness endianess = Endianness.LittleEndian,
      bool assertExactLength = true)
      where T : IBinaryConvertible, new() {
    var bw = new SchemaBinaryWriter(endianess);
    value.Write(bw);

    var actualBytes = await GetEndianBinaryWriterBytes(bw);

    var br = new SchemaBinaryReader(actualBytes, endianess);
    await ReadsAndWritesIdentically<T>(br, assertExactLength);
  }

  public static async Task ReadsAndWritesIdentically<T>(
      IBinaryReader br,
      bool assertExactLength = true)
      where T : IBinaryConvertible, new() {
    var readerStartPos = br.Position;
    var instance = br.ReadNew<T>();

    var expectedReadLength = br.Position - readerStartPos;

    br.Position = readerStartPos;
    var expectedBytes = br.ReadBytes(expectedReadLength);

    var bw = new SchemaBinaryWriter(br.Endianness);
    instance.Write(bw);

    var actualBytes = await GetEndianBinaryWriterBytes(bw);

    if (assertExactLength) {
      Asserts.Equal(expectedReadLength, actualBytes.Length);
    }

    Asserts.Equal(expectedBytes, actualBytes);
  }
}