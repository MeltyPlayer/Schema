using NUnit.Framework;

using schema.text;


namespace System.IO {
  internal class FinTextReaderMatchingTests {
    [Test]
    public void TestReadUpToStartOfTerminator() {
      using var tw = TextSchemaTestUtil.CreateTextReader("abc,,xyz, 123");

      Assert.AreEqual("abc", tw.ReadUpToStartOfTerminator(","));
      tw.AssertString(",");
      Assert.AreEqual(String.Empty, tw.ReadUpToStartOfTerminator(","));
      tw.AssertString(",");
      Assert.AreEqual("xyz", tw.ReadUpToStartOfTerminator(",", " "));
      tw.AssertString(",");
      Assert.AreEqual(" 123", tw.ReadUpToStartOfTerminator(","));
    }

    [Test]
    public void TestReadUpToAndPastTerminator() {
      using var tw = TextSchemaTestUtil.CreateTextReader("abc,,xyz, 123");

      Assert.AreEqual("abc", tw.ReadUpToAndPastTerminator(","));
      Assert.AreEqual(String.Empty, tw.ReadUpToAndPastTerminator(","));
      Assert.AreEqual("xyz", tw.ReadUpToAndPastTerminator(","));
      Assert.AreEqual(" 123", tw.ReadUpToAndPastTerminator(","));
    }

    [Test]
    public void TestReadWhile() {
      using var tw = TextSchemaTestUtil.CreateTextReader("0001111");

      Assert.AreEqual(String.Empty, tw.ReadWhile("a"));
      Assert.AreEqual("000", tw.ReadWhile("0"));
      Assert.AreEqual("1111", tw.ReadWhile("1"));
    }
  }
}