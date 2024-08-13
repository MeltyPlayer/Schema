﻿using NUnit.Framework;

using schema.util.asserts;


namespace schema.text.reader;

internal class SchemaTextReaderNumbersTests {
  [Test]
  public void TestReadBytesNull() {
      using var tr = TextSchemaTestUtil.CreateTextReader("0,, ,255");
      Asserts.Equal(new byte?[] {0, null, null, 255},
                    tr.ReadBytesIncludingEmpty(new[] {","},
                                               TextReaderConstants
                                                   .NEWLINE_STRINGS));
    }

  [Test]
  public void TestReadHexBytesNull() {
      using var tr = TextSchemaTestUtil.CreateTextReader("0x00,, ,0xff");
      Asserts.Equal(new byte?[] {0, null, null, 255},
                    tr.ReadHexBytesIncludingEmpty(
                        new[] {","},
                        TextReaderConstants.NEWLINE_STRINGS));
    }

  [Test]
  [TestCase("", new byte[0])]
  [TestCase("\n0", new byte[0])]
  [TestCase("0", new byte[] {0})]
  [TestCase("0, 255", new byte[] {0, 255})]
  [TestCase("0, , 255", new byte[] {0, 255})]
  [TestCase("0, 255\n1", new byte[] {0, 255})]
  public void TestReadBytes(string inputText, byte[] expectedValues) {
      using var tr = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValues,
                    tr.ReadBytes(new[] {","},
                                 TextReaderConstants.NEWLINE_STRINGS));
    }

  [Test]
  [TestCase("", new byte[0])]
  [TestCase("\n0x00", new byte[0])]
  [TestCase("0x00, 0xff", new byte[] {0, 255})]
  [TestCase("0x00, 0xff\n0x01", new byte[] {0, 255})]
  public void TestReadHexBytes(string inputText, byte[] expectedValues) {
      using var tr = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValues,
                    tr.ReadHexBytes(new[] {","},
                                    TextReaderConstants.NEWLINE_STRINGS));
    }

  [Test]
  [TestCase("", new float[0])]
  [TestCase("\n0", new float[0])]
  [TestCase("-.01, 0.01", new[] {-.01f, 0.01f})]
  public void TestReadSingles(string inputText, float[] expectedValues) {
      using var tr = TextSchemaTestUtil.CreateTextReader(inputText);
      Asserts.Equal(expectedValues,
                    tr.ReadSingles(new[] {","},
                                   TextReaderConstants.NEWLINE_STRINGS));
    }
}