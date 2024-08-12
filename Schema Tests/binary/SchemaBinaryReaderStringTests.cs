using System.Text;

using NUnit.Framework;

using schema.testing;


namespace schema.binary {
  public class SchemaBinaryReaderStringTests {
    [Test]
    public void TestAssertChar() {
      var br = new SchemaBinaryReader(SchemaMemoryStream.From(['c']));
      br.AssertChar('c');
    }

    [Test]
    public void TestAssertCharUtf16() {
      var br = new SchemaBinaryReader(SchemaMemoryStream.From(['é']));
      br.AssertChar(Encoding.Unicode, 'é');
    }

    [Test]
    public void TestAssertString() {
      var br = new SchemaBinaryReader(SchemaMemoryStream.From("cat"));
      br.AssertString("cat");
    }

    [Test]
    public void TestAssertStringUtf16() {
      var br = new SchemaBinaryReader(
          SchemaMemoryStream.From("félin", Encoding.Unicode));
      br.AssertString(Encoding.Unicode, "félin");
    }

    [Test]
    public void TestAssertStringNT() {
      var br = new SchemaBinaryReader(SchemaMemoryStream.From("cat\0"));
      br.AssertStringNT("cat");
    }

    [Test]
    public void TestAssertStringNTUtf16() {
      var br = new SchemaBinaryReader(
          SchemaMemoryStream.From($"félin\0", Encoding.Unicode));
      br.AssertStringNT(Encoding.Unicode, "félin");
    }

    [Test]
    [TestCase("\n")]
    [TestCase("\r\n")]
    public void TestReadLine(string newline) {
      var br = new SchemaBinaryReader(SchemaMemoryStream.From($"cat{newline}"));
      Assert.AreEqual("cat", br.ReadLine());
    }

    [Test]
    [TestCase("\n")]
    [TestCase("\r\n")]
    public void TestReadLineUtf16(string newline) {
      var br = new SchemaBinaryReader(
          SchemaMemoryStream.From($"félin{newline}", Encoding.Unicode));
      Assert.AreEqual("félin", br.ReadLine(Encoding.Unicode));
    }
  }
}