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
      Assert.AreEqual(sizeof(byte), ms.Position);
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
      Assert.AreEqual(sizeof(sbyte), ms.Position);
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
      Assert.AreEqual(sizeof(short), ms.Position);
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
      Assert.AreEqual(sizeof(ushort), ms.Position);
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
      Assert.AreEqual(sizeof(int), ms.Position);
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
      Assert.AreEqual(sizeof(uint), ms.Position);
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
      Assert.AreEqual(sizeof(long), ms.Position);
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
      Assert.AreEqual(sizeof(ulong), ms.Position);
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00 }, 0f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x20 }, .25f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x40 }, .5f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x60 }, .75f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x7F }, 1f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x80 }, -1f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xA0 }, -.75f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xC0 }, -.5f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xE0 }, -.25f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF }, 0f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00 }, 0f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x20 }, .25f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x40 }, .5f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x60 }, .75f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F }, 1f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80 }, -1f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xA0 }, -.75f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xC0 }, -.5f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xE0 }, -.25f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF }, 0f)]
    public void TestReadSn8(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadSn8(), .01f);
      Assert.AreEqual(sizeof(sbyte), ms.Position);
    }

    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00 }, 0f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x40 }, .25f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x80 }, .5f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xC0 }, .75f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF }, 1f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00 }, 0f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x40 }, .25f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80 }, .5f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xC0 }, .75f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF }, 1f)]
    public void TestReadUn8(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadUn8(), .01f);
      Assert.AreEqual(sizeof(byte), ms.Position);
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00 }, 0f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x20 }, .25f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x40 }, .5f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x60 }, .75f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0x7F }, 1f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x80 }, -1f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0xA0 }, -.75f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0xC0 }, -.5f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0xE0 }, -.25f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF }, 0f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00 }, 0f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x20, 0x00 }, .25f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x40, 0x00 }, .5f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x60, 0x00 }, .75f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF }, 1f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00 }, -1f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xA0, 0x00 }, -.75f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xC0, 0x00 }, -.5f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xE0, 0x00 }, -.25f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF }, 0f)]
    public void TestReadSn16(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadSn16(), .01f);
      Assert.AreEqual(sizeof(short), ms.Position);
    }

    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00 }, 0f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x40 }, .25f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x80 }, .5f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0xC0 }, .75f)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF }, 1f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00 }, 0f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x40, 0x00 }, .25f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00 }, .5f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xC0, 0x00 }, .75f)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF }, 1f)]
    public void TestReadUn16(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      Assert.AreEqual(expectedValue, er.ReadUn16(), .01f);
      Assert.AreEqual(sizeof(ushort), ms.Position);
    }
  }
}