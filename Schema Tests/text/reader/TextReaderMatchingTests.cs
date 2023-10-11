using NUnit.Framework;

namespace schema.text.reader {
  internal class TextReaderMatchingTests {
    [Test]
    public void TestReadUpToStartOfTerminator() {
      using var tr = TextSchemaTestUtil.CreateTextReader("abc,,xyz, 123");

      Assert.AreEqual("abc", tr.ReadUpToStartOfTerminator(","));
      tr.AssertString(",");
      Assert.AreEqual(string.Empty, tr.ReadUpToStartOfTerminator(","));
      tr.AssertString(",");
      Assert.AreEqual("xyz", tr.ReadUpToStartOfTerminator(",", " "));
      tr.AssertString(",");
      Assert.AreEqual(" 123", tr.ReadUpToStartOfTerminator(","));
    }

    [Test]
    public void TestReadUpToAndPastTerminator() {
      using var tr = TextSchemaTestUtil.CreateTextReader("abc,,xyz, 123");

      Assert.AreEqual("abc", tr.ReadUpToAndPastTerminator(","));
      Assert.AreEqual(string.Empty, tr.ReadUpToAndPastTerminator(","));
      Assert.AreEqual("xyz", tr.ReadUpToAndPastTerminator(","));
      Assert.AreEqual(" 123", tr.ReadUpToAndPastTerminator(","));
    }

    [Test]
    public void TestReadWhile() {
      using var tr = TextSchemaTestUtil.CreateTextReader("0001111");

      Assert.AreEqual(string.Empty, tr.ReadWhile("a"));
      Assert.AreEqual("000", tr.ReadWhile("0"));
      Assert.AreEqual("1111", tr.ReadWhile("1"));
    }
  }
}