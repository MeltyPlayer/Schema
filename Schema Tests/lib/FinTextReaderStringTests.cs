using NUnit.Framework;

using schema.text;


namespace System.IO {
  internal class FinTextReaderStringTests {
    [Test]
    public void TestReadChar() {
      using var tw = TextSchemaTestUtil.CreateTextReader("abc");

      Assert.AreEqual('a', tw.ReadChar());
      Assert.AreEqual('b', tw.ReadChar());
      Assert.AreEqual('c', tw.ReadChar());
    }

    [Test]
    public void TestAssertChar() {
      using var tw = TextSchemaTestUtil.CreateTextReader("abc");

      tw.AssertChar('a');
      tw.AssertChar('b');
      tw.AssertChar('c');
    }

    [Test]
    public void TestReadStrings() {
      using var tw = TextSchemaTestUtil.CreateTextReader("abc,,xyz, 123");
      Assert.AreEqual(new[] { "abc", String.Empty, "xyz", " 123" },
                      tw.ReadStrings(new[] { "," }, new[] { "\n" }));
    }
  }
}