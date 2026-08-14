using System;

using NUnit.Framework;


namespace schema.text.reader;

internal class SchemaTextReaderStringTests {
  [Test]
  public void TestReadChar() {
    using var tr = new SchemaTextReader("abc");

    Assert.AreEqual('a', tr.ReadChar());
    Assert.AreEqual('b', tr.ReadChar());
    Assert.AreEqual('c', tr.ReadChar());
  }

  [Test]
  public void TestAssertChar() {
    using var tr = new SchemaTextReader("abc");

    tr.AssertChar('a');
    tr.AssertChar('b');
    tr.AssertChar('c');
  }

  [Test]
  public void TestReadStrings() {
    using var tr = new SchemaTextReader("abc,,xyz, 123");
    Assert.AreEqual(new[] { "abc", String.Empty, "xyz", " 123" },
                    tr.ReadStrings(new[] { "," }, new[] { "\n" }));
  }
}