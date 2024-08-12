using System.Threading.Tasks;

using NUnit.Framework;

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
      var asciiWrapper = new AsciiWrapper {Text = text};
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
      var utf8Wrapper = new Utf8Wrapper {Text = text};
      await BinarySchemaAssert.WritesAndReadsIdentically(utf8Wrapper);
    }


    [BinarySchema]
    public partial class Utf16Wrapper : IBinaryConvertible {
      [StringEncoding(StringEncodingType.UTF16)]
      public string Text { get; set; }
    }

    [Test]
    [TestCase("", Endianness.LittleEndian)]
    [TestCase("", Endianness.BigEndian)]
    [TestCase("Foo bar", Endianness.LittleEndian)]
    [TestCase("Foo bar", Endianness.BigEndian)]
    [TestCase("Hello world!", Endianness.LittleEndian)]
    [TestCase("Hello world!", Endianness.BigEndian)]
    [TestCase("Multiple\nLines", Endianness.LittleEndian)]
    [TestCase("Multiple\nLines", Endianness.BigEndian)]
    [TestCase("你好世界", Endianness.LittleEndian)]
    [TestCase("你好世界", Endianness.BigEndian)]
    public async Task TestUtf16(string text, Endianness endianness) {
      var utf16Wrapper = new Utf16Wrapper {Text = text};
      await BinarySchemaAssert.WritesAndReadsIdentically(
          utf16Wrapper,
          endianness);
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
      var utf32Wrapper = new Utf32Wrapper {Text = text};
      await BinarySchemaAssert.WritesAndReadsIdentically(utf32Wrapper);
    }
  }
}