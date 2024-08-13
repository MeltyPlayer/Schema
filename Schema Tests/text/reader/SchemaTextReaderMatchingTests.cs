using NUnit.Framework;


namespace schema.text.reader;

internal class SchemaTextReaderMatchingTests {
  [Test]
  public void TestReadUpToStartOfTerminator() {
      using var tr = TextSchemaTestUtil.CreateTextReader("abc,,xyz, 123");

      Assert.AreEqual("abc", tr.ReadUpToStartOfTerminator(new[] {","}));
      tr.AssertString(",");
      Assert.AreEqual(string.Empty,
                      tr.ReadUpToStartOfTerminator(new[] {","}));
      tr.AssertString(",");
      Assert.AreEqual("xyz", tr.ReadUpToStartOfTerminator(new[] {",", " "}));
      tr.AssertString(",");
      Assert.AreEqual(" 123", tr.ReadUpToStartOfTerminator(new[] {","}));
    }

  [Test]
  public void TestReadUpToAndPastTerminator() {
      using var tr = TextSchemaTestUtil.CreateTextReader("abc,,xyz, 123");

      Assert.AreEqual("abc", tr.ReadUpToAndPastTerminator(new[] {","}));
      Assert.AreEqual(string.Empty,
                      tr.ReadUpToAndPastTerminator(new[] {","}));
      Assert.AreEqual("xyz", tr.ReadUpToAndPastTerminator(new[] {","}));
      Assert.AreEqual(" 123", tr.ReadUpToAndPastTerminator(new[] {","}));
    }

  [Test]
  public void TestReadWhile() {
      using var tr = TextSchemaTestUtil.CreateTextReader("0001111");

      Assert.AreEqual(string.Empty, tr.ReadWhile("a"));
      Assert.AreEqual("000", tr.ReadWhile("0"));
      Assert.AreEqual("1111", tr.ReadWhile("1"));
    }
}