using NUnit.Framework;


namespace System.IO {
  public class EndianBinaryReaderNumbersTests {
    [Test]
    [TestCase(new byte[] { 0x00, 0x00 }, 0)]
    [TestCase(new byte[] { 0xFF, 0xFF }, -1)]
    [TestCase(new byte[] { 0xFF, 0x7F }, short.MaxValue)]
    [TestCase(new byte[] { 0x00, 0x80 }, short.MinValue)]
    public void TestReadInt16(byte[] bytes, short expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, Endianness.LittleEndian);
      Assert.AreEqual(expectedValue, er.ReadInt16());
      Assert.AreEqual(2, ms.Position);
    }

    [Test]
    [TestCase(new byte[] { 0x00, 0x00 }, (ushort) 0)]
    [TestCase(new byte[] { 0xFF, 0xFF }, ushort.MaxValue)]
    [TestCase(new byte[] { 0xFF, 0x7F }, (ushort) 32767)]
    [TestCase(new byte[] { 0x00, 0x80 }, (ushort) 32768)]
    public void TestReadUInt16(byte[] bytes, ushort expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, Endianness.LittleEndian);
      Assert.AreEqual(expectedValue, er.ReadUInt16());
      Assert.AreEqual(2, ms.Position);
    }


    [Test]
    [TestCase(new byte[] { 0x00, 0x00, 0x00 }, 0)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0x7F }, 8388607)]
    [TestCase(new byte[] { 0x00, 0x00, 0x80 }, -8388608)]
    public void TestReadInt24(byte[] bytes, int expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, Endianness.LittleEndian);
      Assert.AreEqual(expectedValue, er.ReadInt24());
      Assert.AreEqual(3, ms.Position);
    }

    [Test]
    [TestCase(new byte[] { 0x00, 0x00, 0x00 }, (uint) 0)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF }, (uint) 16777215)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0x7F }, (uint) 8388607)]
    [TestCase(new byte[] { 0x00, 0x00, 0x80 }, (uint) 8388608)]
    public void TestReadUInt24(byte[] bytes, uint expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, Endianness.LittleEndian);
      Assert.AreEqual(expectedValue, er.ReadUInt24());
      Assert.AreEqual(3, ms.Position);
    }
  }
}