using System.IO;
using System.Threading.Tasks;

using schema.binary;
using schema.binary.attributes;
using schema.binary.testing;


namespace build {
  internal partial class EncodingTests {
    [BinarySchema]
    public partial class AsciiWrapper : IBinaryConvertible {
      [StringEncoding(StringEncodingType.ASCII)]
      public string Text { get; set; }
    }

    [Test]
    [TestCase("")]
    [TestCase("Foo bar")]
    [TestCase("Hello world!")]
    [TestCase("Multiple\nLines")]
    public async Task TestAscii(string text) {
      var asciiWrapper = new AsciiWrapper { Text = text };
      await BinarySchemaAssert.WritesAndReadsIdentically(asciiWrapper);
    }


    [BinarySchema]
    public partial class Utf8Wrapper : IBinaryConvertible {
      [StringEncoding(StringEncodingType.UTF8)]
      public string Text { get; set; }
    }

    [Test]
    [TestCase("")]
    [TestCase("Foo bar")]
    [TestCase("Hello world!")]
    [TestCase("Multiple\nLines")]
    [TestCase("你好世界")]
    public async Task TestUtf8(string text) {
      var utf8Wrapper = new Utf8Wrapper { Text = text };
      await BinarySchemaAssert.WritesAndReadsIdentically(utf8Wrapper);
    }


    [BinarySchema]
    public partial class Utf16Wrapper : IBinaryConvertible {
      [StringEncoding(StringEncodingType.UTF16)]
      public string Text { get; set; }
    }

    [Test]
    [TestCase("")]
    [TestCase("Foo bar")]
    [TestCase("Hello world!")]
    [TestCase("Multiple\nLines")]
    [TestCase("你好世界")]
    public async Task TestUtf16(string text) {
      var utf16Wrapper = new Utf16Wrapper { Text = text };
      await BinarySchemaAssert.WritesAndReadsIdentically(utf16Wrapper);
    }


    [BinarySchema]
    public partial class Utf32Wrapper : IBinaryConvertible {
      [StringEncoding(StringEncodingType.UTF32)]
      public string Text { get; set; }
    }

    [Test]
    [TestCase("")]
    [TestCase("Foo bar")]
    [TestCase("Hello world!")]
    [TestCase("Multiple\nLines")]
    [TestCase("你好世界")]
    public async Task TestUtf32(string text) {
      var utf32Wrapper = new Utf32Wrapper { Text = text };
      await BinarySchemaAssert.WritesAndReadsIdentically(utf32Wrapper);
    }
  }
}