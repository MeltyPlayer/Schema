using NUnit.Framework;

using schema.text;


namespace System.IO {
  internal class FinTextReaderPositionTests {
    [Test]
    public void TestGetPosition() {
      using var tw = TextSchemaTestUtil.CreateTextReader("abc");

      Assert.AreEqual(0, tw.Position);
      Assert.AreEqual('a', tw.ReadChar());
      Assert.AreEqual(1, tw.Position);
    }

    [Test]
    public void TestSetPosition() {
      using var tw = TextSchemaTestUtil.CreateTextReader("abc");

      tw.Position = 1;
      Assert.AreEqual('b', tw.ReadChar());
    }

    [Test]
    public void TestLength() {
      using var tw = TextSchemaTestUtil.CreateTextReader("abc");
      Assert.AreEqual(3, tw.Length);
    }
  }
}