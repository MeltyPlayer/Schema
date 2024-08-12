using NUnit.Framework;


namespace schema.text.reader {
  internal class SchemaTextReaderPositionTests {
    [Test]
    public void TestGetPositions() {
      using var tr = TextSchemaTestUtil.CreateTextReader("abc");

      Assert.AreEqual(0, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(0, tr.IndexInLine);

      Assert.AreEqual('a', tr.ReadChar());
      Assert.AreEqual(1, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(1, tr.IndexInLine);
    }

    [Test]
    public void TestGetPositionsWithTabs() {
      using var tr = TextSchemaTestUtil.CreateTextReader("\t1\t");

      Assert.AreEqual(0, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(0, tr.IndexInLine);

      Assert.AreEqual('\t', tr.ReadChar());
      Assert.AreEqual(1, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(4, tr.IndexInLine);

      Assert.AreEqual('1', tr.ReadChar());
      Assert.AreEqual(2, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(5, tr.IndexInLine);

      Assert.AreEqual('\t', tr.ReadChar());
      Assert.AreEqual(3, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(8, tr.IndexInLine);
    }

    [Test]
    public void TestGetPositionsAcrossLines() {
      using var tr = TextSchemaTestUtil.CreateTextReader("ab\n12\nfoo");

      Assert.AreEqual(0, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(0, tr.IndexInLine);

      Assert.AreEqual('a', tr.ReadChar());
      Assert.AreEqual(1, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(1, tr.IndexInLine);

      Assert.AreEqual('b', tr.ReadChar());
      Assert.AreEqual(2, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(2, tr.IndexInLine);

      Assert.AreEqual('\n', tr.ReadChar());
      Assert.AreEqual(3, tr.Position);
      Assert.AreEqual(1, tr.LineNumber);
      Assert.AreEqual(0, tr.IndexInLine);

      Assert.AreEqual('1', tr.ReadChar());
      Assert.AreEqual(4, tr.Position);
      Assert.AreEqual(1, tr.LineNumber);
      Assert.AreEqual(1, tr.IndexInLine);

      Assert.AreEqual('2', tr.ReadChar());
      Assert.AreEqual(5, tr.Position);
      Assert.AreEqual(1, tr.LineNumber);
      Assert.AreEqual(2, tr.IndexInLine);

      Assert.AreEqual('\n', tr.ReadChar());
      Assert.AreEqual(6, tr.Position);
      Assert.AreEqual(2, tr.LineNumber);
      Assert.AreEqual(0, tr.IndexInLine);
    }

    [Test]
    public void TestGetPositionsAcrossLinesWhenReadingMultiple() {
      var text = "abc\n\t1\t23\nfoo";

      using var tr = TextSchemaTestUtil.CreateTextReader("abc\n\t1\t23\nfoo");


      Assert.AreEqual(0, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(0, tr.IndexInLine);

      Assert.AreEqual("abc", tr.ReadString(3));
      Assert.AreEqual(3, tr.Position);
      Assert.AreEqual(0, tr.LineNumber);
      Assert.AreEqual(3, tr.IndexInLine);

      Assert.AreEqual("\n", tr.ReadString(1));
      Assert.AreEqual(4, tr.Position);
      Assert.AreEqual(1, tr.LineNumber);
      Assert.AreEqual(0, tr.IndexInLine);

      Assert.AreEqual("\t1\t23", tr.ReadString(5));
      Assert.AreEqual(9, tr.Position);
      Assert.AreEqual(1, tr.LineNumber);
      Assert.AreEqual(10, tr.IndexInLine);

      Assert.AreEqual("\n", tr.ReadString(1));
      Assert.AreEqual(10, tr.Position);
      Assert.AreEqual(2, tr.LineNumber);
      Assert.AreEqual(0, tr.IndexInLine);

      Assert.AreEqual("foo", tr.ReadString(3));
      Assert.AreEqual(13, tr.Position);
      Assert.AreEqual(2, tr.LineNumber);
      Assert.AreEqual(3, tr.IndexInLine);
    }

    [Test]
    public void TestLength() {
      using var tr = TextSchemaTestUtil.CreateTextReader("abc");
      Assert.AreEqual(3, tr.Length);
    }
  }
}