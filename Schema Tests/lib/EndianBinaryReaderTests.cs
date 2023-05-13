using NUnit.Framework;


namespace System.IO {
  public class EndianBinaryReaderTests {
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
    }

    [Test]
    public void TestReadInt24() {
      using var zeroStream = new MemoryStream(new byte[] {0x00, 0x00, 0x00});
      using var zeroEr = new EndianBinaryReader(zeroStream, Endianness.LittleEndian);
      Assert.AreEqual(0, zeroEr.ReadInt24());

      using var maxStream = new MemoryStream(new byte[] { 0xFF, 0xFF, 0xFF });
      using var maxEr = new EndianBinaryReader(maxStream, Endianness.LittleEndian);
      Assert.AreEqual(-1, maxEr.ReadInt24());

      using var halfStream = new MemoryStream(new byte[] { 0xFF, 0xFF, 0x7F });
      using var halfEr = new EndianBinaryReader(halfStream, Endianness.LittleEndian);
      Assert.AreEqual(8388607, halfEr.ReadInt24());

      using var halfPlus1Stream = new MemoryStream(new byte[] { 0x00, 0x00, 0x80 });
      using var halfPlus1Er = new EndianBinaryReader(halfPlus1Stream, Endianness.LittleEndian);
      Assert.AreEqual(-8388608, halfPlus1Er.ReadInt24());
    }

    [Test]
    public void TestReadUInt24() {
      using var zeroStream = new MemoryStream(new byte[] { 0x00, 0x00, 0x00 });
      using var zeroEr = new EndianBinaryReader(zeroStream, Endianness.LittleEndian);
      Assert.AreEqual(0, zeroEr.ReadUInt24());

      using var maxStream = new MemoryStream(new byte[] { 0xFF, 0xFF, 0xFF });
      using var maxEr = new EndianBinaryReader(maxStream, Endianness.LittleEndian);
      Assert.AreEqual(16777215, maxEr.ReadUInt24());

      using var halfStream = new MemoryStream(new byte[] { 0xFF, 0xFF, 0x7F });
      using var halfEr = new EndianBinaryReader(halfStream, Endianness.LittleEndian);
      Assert.AreEqual(8388607, halfEr.ReadUInt24());

      using var halfPlus1Stream = new MemoryStream(new byte[] { 0x00, 0x00, 0x80 });
      using var halfPlus1Er = new EndianBinaryReader(halfPlus1Stream, Endianness.LittleEndian);
      Assert.AreEqual(8388608, halfPlus1Er.ReadUInt24());
    }
  }
}