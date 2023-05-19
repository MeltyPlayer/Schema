using NUnit.Framework;


namespace System.IO {
  public class EndianBinaryReaderReadNumberTests {
    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00 }, byte.MinValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF }, byte.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x7F }, (byte) 127)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x80 }, (byte) 128)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00 }, byte.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF }, byte.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F }, (byte) 127)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80 }, (byte) 128)]
    public void TestReadByte(Endianness endianness, byte[] bytes, byte expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadByte());
      Assert.AreEqual(1, ms.Position);
    }

    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF }, -1)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x7F }, sbyte.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x80 }, sbyte.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF }, -1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F }, sbyte.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80 }, sbyte.MinValue)]
    public void TestReadSByte(Endianness endianness, byte[] bytes, sbyte expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadSByte());
      Assert.AreEqual(1, ms.Position);
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0x7F }, short.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x80 }, short.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF }, short.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00 }, short.MinValue)]
    public void TestReadInt16(Endianness endianness, byte[] bytes, short expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadInt16());
      Assert.AreEqual(2, ms.Position);
    }

    [Test]
    [TestCase(Endianness.LittleEndian,new byte[] { 0x00, 0x00 }, ushort.MinValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF }, ushort.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0x7F }, (ushort) 32767)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x80 }, (ushort) 32768)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00 }, ushort.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF }, ushort.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF }, (ushort) 32767)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00 }, (ushort) 32768)]
    public void TestReadUInt16(Endianness endianness, byte[] bytes, ushort expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadUInt16());
      Assert.AreEqual(2, ms.Position);
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0x7F }, 8388607)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x80 }, -8388608)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF, 0xFF }, 8388607)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00, 0x00 }, -8388608)]
    public void TestReadInt24(Endianness endianness, byte[] bytes, int expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadInt24());
      Assert.AreEqual(3, ms.Position);
    }

    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00 }, (uint) 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF }, (uint) 16777215)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0x7F }, (uint) 8388607)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x80 }, (uint) 8388608)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00 }, (uint) 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF, 0xFF }, (uint) 16777215)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF, 0xFF }, (uint) 8388607)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00, 0x00 }, (uint) 8388608)]
    public void TestReadUInt24(Endianness endianness, byte[] bytes, uint expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadUInt24());
      Assert.AreEqual(3, ms.Position);
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, int.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x80 }, int.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, int.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00, 0x00, 0x00 }, int.MinValue)]
    public void TestReadInt32(Endianness endianness, byte[] bytes, int expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadInt32());
      Assert.AreEqual(4, ms.Position);
    }

    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00 }, uint.MinValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, uint.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, (uint) 2147483647)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x80 }, (uint) 2147483648)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00, 0x00 }, uint.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, uint.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, (uint) 2147483647)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00, 0x00, 0x00 }, (uint) 2147483648)]
    public void TestReadUInt32(Endianness endianness, byte[] bytes, uint expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadUInt32());
      Assert.AreEqual(4, ms.Position);
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, long.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, long.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, long.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, long.MinValue)]
    public void TestReadInt64(Endianness endianness, byte[] bytes, long expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadInt64());
      Assert.AreEqual(8, ms.Position);
    }

    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, ulong.MinValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, ulong.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, (ulong) 9223372036854775807)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, (ulong) 9223372036854775808)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, ulong.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, ulong.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, (ulong) 9223372036854775807)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, (ulong) 9223372036854775808)]
    public void TestReadUInt64(Endianness endianness, byte[] bytes, ulong expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadUInt64());
      Assert.AreEqual(8, ms.Position);
    }
  }
}