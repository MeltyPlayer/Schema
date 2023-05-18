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
    [TestCase(new byte[] { 0x00, 0x00 }, ushort.MinValue)]
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


    [Test]
    [TestCase(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, int.MaxValue)]
    [TestCase(new byte[] { 0x00, 0x00, 0x00, 0x80 }, int.MinValue)]
    public void TestReadInt32(byte[] bytes, int expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, Endianness.LittleEndian);
      Assert.AreEqual(expectedValue, er.ReadInt32());
      Assert.AreEqual(4, ms.Position);
    }

    [Test]
    [TestCase(new byte[] { 0x00, 0x00, 0x00, 0x00 }, uint.MinValue)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, uint.MaxValue)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, (uint) 2147483647)]
    [TestCase(new byte[] { 0x00, 0x00, 0x00, 0x80 }, (uint) 2147483648)]
    public void TestReadUInt32(byte[] bytes, uint expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, Endianness.LittleEndian);
      Assert.AreEqual(expectedValue, er.ReadUInt32());
      Assert.AreEqual(4, ms.Position);
    }


    [Test]
    [TestCase(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, long.MaxValue)]
    [TestCase(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, long.MinValue)]
    public void TestReadInt64(byte[] bytes, long expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, Endianness.LittleEndian);
      Assert.AreEqual(expectedValue, er.ReadInt64());
      Assert.AreEqual(8, ms.Position);
    }

    [Test]
    [TestCase(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, ulong.MinValue)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, ulong.MaxValue)]
    [TestCase(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, (ulong) 9223372036854775807)]
    [TestCase(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, (ulong) 9223372036854775808)]
    public void TestReadUInt64(byte[] bytes, ulong expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, Endianness.LittleEndian);
      Assert.AreEqual(expectedValue, er.ReadUInt64());
      Assert.AreEqual(8, ms.Position);
    }
  }
}