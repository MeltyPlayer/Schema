using System.IO;

using NUnit.Framework;


namespace schema.binary;

public class SchemaBinaryReaderStringAsciiTests {
  [Test]
  [TestCase("foobar")]
  public void TestReadChars(string str) {
    using var ms = new MemoryStream();
    using var sw = new StreamWriter(ms);
    sw.Write(str);
    sw.Flush();
    ms.Position = 0;


    using var br = new SchemaBinaryReader(ms);
    CollectionAssert.AreEqual(str, br.ReadChars(str.Length));
    Assert.AreEqual(str.Length, ms.Position);
  }

  [Test]
  [TestCase("foobar")]
  public void TestString(string str) {
    using var ms = new MemoryStream();
    using var sw = new StreamWriter(ms);
    sw.Write(str);
    sw.Flush();
    ms.Position = 0;

    using var br = new SchemaBinaryReader(ms);
    Assert.AreEqual(str, br.ReadString(str.Length));
    Assert.AreEqual(str.Length, ms.Position);
  }

  [Test]
  public void TestReadNT() {
    var str = "string 1\0string 2\0string 3";

    using var ms = new MemoryStream();
    using var sw = new StreamWriter(ms);
    sw.Write(str);
    sw.Flush();
    ms.Position = 0;

    using var br = new SchemaBinaryReader(ms);
    Assert.AreEqual("string 1", br.ReadStringNT());
    Assert.AreEqual("string 2", br.ReadStringNT());
    Assert.AreEqual("string 3", br.ReadStringNT());
    Assert.AreEqual(str.Length, ms.Position);
  }

  [Test]
  public void TestReadLines() {
    var str = "line 1\nline 2\r\nline 3";

    using var sr = new StringReader(str);
    Assert.AreEqual("line 1", sr.ReadLine());
    Assert.AreEqual("line 2", sr.ReadLine());
    Assert.AreEqual("line 3", sr.ReadLine());

    using var ms = new MemoryStream();
    using var sw = new StreamWriter(ms);
    sw.Write(str);
    sw.Flush();
    ms.Position = 0;

    using var br = new SchemaBinaryReader(ms);
    Assert.AreEqual("line 1", br.ReadLine());
    Assert.AreEqual("line 2", br.ReadLine());
    Assert.AreEqual("line 3", br.ReadLine());
    Assert.AreEqual(str.Length, ms.Position);
  }
}