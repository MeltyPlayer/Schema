using NUnit.Framework;

using schema.binary.util;
using schema.text;


namespace System.IO {
  internal class FinTextReaderNumberTests {
    [Test]
    [TestCase(" 0", 0)]
    [TestCase("0", 0)]
    [TestCase("255", 255)]
    public void TestReadByte(string inputText, byte expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadByte());
    }

    [Test]
    [TestCase(" 0x00", 0)]
    [TestCase("0x00", 0)]
    [TestCase("0xFF", 255)]
    [TestCase("0xff", 255)]
    [TestCase("0Xff", 255)]
    [TestCase("ff", 255)]
    public void TestReadHexByte(string inputText, byte expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadHexByte());
    }

    [Test]
    [TestCase("0", 0)]
    [TestCase("127", 127)]
    [TestCase("-128", -128)]
    public void TestReadSByte(string inputText, sbyte expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadSByte());
    }

    [Test]
    [TestCase("0x00", 0)]
    [TestCase("0xFF", -1)]
    public void TestReadHexSByte(string inputText, sbyte expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadHexSByte());
    }

    [Test]
    [TestCase("0", 0)]
    [TestCase("12345", 12345)]
    public void TestReadInt16(string inputText, short expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadInt16());
    }

    [Test]
    [TestCase("0", (ushort) 0)]
    [TestCase("12345", (ushort) 12345)]
    public void TestReadUInt16(string inputText, ushort expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadUInt16());
    }

    [Test]
    [TestCase("0", 0)]
    [TestCase("1234567", 1234567)]
    public void TestReadInt32(string inputText, int expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadInt32());
    }

    [Test]
    [TestCase("0", (uint) 0)]
    [TestCase("1234567", (uint) 1234567)]
    public void TestReadUInt32(string inputText, uint expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadUInt32());
    }

    [Test]
    [TestCase("0", 0)]
    [TestCase("123456789", 123456789)]
    public void TestReadInt64(string inputText, long expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadInt64());
    }

    [Test]
    [TestCase("0", (ulong) 0)]
    [TestCase("123456789", (ulong) 123456789)]
    public void TestReadUInt64(string inputText, ulong expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadUInt64());
    }

    [Test]
    [TestCase(" 0", 0)]
    [TestCase("1", 1)]
    [TestCase("0.01", 0.01f)]
    [TestCase("-0.01", -0.01f)]
    public void TestReadSingle(string inputText, float expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadSingle());
    }

    [Test]
    [TestCase("0", 0)]
    [TestCase("1", 1)]
    [TestCase("0.01", 0.01)]
    [TestCase("-0.01", -0.01)]
    public void TestReadDouble(string inputText, double expectedValue) {
      using var tw = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValue, tw.ReadDouble());
    }
  }
}