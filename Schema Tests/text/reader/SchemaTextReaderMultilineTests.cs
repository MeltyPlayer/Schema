using NUnit.Framework;


namespace schema.text.reader;

internal class SchemaTextReaderMultilineTests {
  [Test]
  public void TestReadAcrossMultipleLinesSeparately() {
    var inputText = "1 2 3\n4, 5, 6\n7\n8\n9\nfoobar";

    using var tr = TextSchemaTestUtil.CreateTextReader(inputText);
    Assert.AreEqual(1, tr.ReadInt32());
    Assert.AreEqual(2, tr.ReadInt32());
    Assert.AreEqual(3, tr.ReadInt32());

    Assert.AreEqual(4, tr.ReadInt32());
    Assert.AreEqual(5, tr.ReadInt32());
    Assert.AreEqual(6, tr.ReadInt32());

    Assert.AreEqual(7, tr.ReadInt32());
    Assert.AreEqual(8, tr.ReadInt32());
    Assert.AreEqual(9, tr.ReadInt32());

    Assert.AreEqual("foobar", tr.ReadLine());
  }

  [Test]
  public void TestReadAcrossMultipleLinesCombined() {
    var inputText = "1 2 3\nfoobar";

    using var tr = TextSchemaTestUtil.CreateTextReader(inputText);
    Assert.AreEqual(new[] { 1, 2, 3 },
                    tr.ReadInt32s(
                        TextReaderConstants.WHITESPACE_CHARS,
                        TextReaderConstants.NEWLINE_CHARS));

    tr.AssertChar('\n');

    Assert.AreEqual("foobar", tr.ReadLine());
  }
}