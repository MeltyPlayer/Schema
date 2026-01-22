using System.IO;
using System.Text;

using NUnit.Framework;

using schema.binary.attributes;


namespace schema.binary;

public class StringShiftJisTests {
  private Encoding shiftJis_;

  [OneTimeSetUp]
  public void SetUp() {
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    this.shiftJis_ = Encoding.GetEncoding(932);
  }
  
  [Test]
  [TestCase("foobar")]
  [TestCase("フーバー")]
  public void TestReadChars(string str) {
    using var ms = new MemoryStream();
    using var sw = new StreamWriter(ms, this.shiftJis_);
    sw.Write(str);
    sw.Flush();
    ms.Position = 0;

    using var br = new SchemaBinaryReader(ms);
    CollectionAssert.AreEqual(str, br.ReadChars(StringEncodingType.SHIFT_JIS, str.Length));
  }

  [Test]
  [TestCase("foobar")]
  [TestCase("フーバー")]
  public void TestString(string str) {
    using var ms = new MemoryStream();
    using var sw = new StreamWriter(ms, this.shiftJis_);
    sw.Write(str);
    sw.Flush();
    ms.Position = 0;

    using var br = new SchemaBinaryReader(ms);
    Assert.AreEqual(str, br.ReadString(StringEncodingType.SHIFT_JIS, str.Length));
  }

  [Test]
  public void TestReadNT() {
    var str = "string 1\0string 2\0string 3";

    using var ms = new MemoryStream();
    using var sw = new StreamWriter(ms, this.shiftJis_);
    sw.Write(str);
    sw.Flush();
    ms.Position = 0;

    using var br = new SchemaBinaryReader(ms);
    Assert.AreEqual("string 1", br.ReadStringNT(StringEncodingType.SHIFT_JIS));
    Assert.AreEqual("string 2", br.ReadStringNT(StringEncodingType.SHIFT_JIS));
    Assert.AreEqual("string 3", br.ReadStringNT(StringEncodingType.SHIFT_JIS));
    Assert.AreEqual(str.Length, ms.Position);
  }
}