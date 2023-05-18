using NUnit.Framework;


namespace System.IO {
  public class EndianBinaryReaderStringTests {
    [Test]
    public void TestReadNT() {
      var str = "string 1\0string 2\0string 3";

      using var ms = new MemoryStream();
      using var sw = new StreamWriter(ms);
      sw.Write(str);
      sw.Flush();
      ms.Position = 0;

      using var er = new EndianBinaryReader(ms);
      Assert.AreEqual("string 1", er.ReadStringNT());
      Assert.AreEqual("string 2", er.ReadStringNT());
      Assert.AreEqual("string 3", er.ReadStringNT());
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

      using var er = new EndianBinaryReader(ms);
      Assert.AreEqual("line 1", er.ReadLine());
      Assert.AreEqual("line 2", er.ReadLine());
      Assert.AreEqual("line 3", er.ReadLine());
      Assert.AreEqual(str.Length, ms.Position);
    }
  }
}