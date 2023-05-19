using NUnit.Framework;


namespace System.IO {
  public class EndianBinaryReaderAssertNumberTests {
    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00 }, byte.MinValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF }, byte.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x7F }, (byte) 127)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x80 }, (byte) 128)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00 }, byte.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF }, byte.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F }, (byte) 127)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80 }, (byte) 128)]
    public void TestAssertByte(Endianness endianness, byte[] bytes, byte expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);
      
      Assert.DoesNotThrow(() => er.AssertByte(expectedValue));
      Assert.AreEqual(sizeof(byte), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertByte((byte) (~expectedValue)));
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
    public void TestAssertSByte(Endianness endianness, byte[] bytes, sbyte expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertSByte(expectedValue));
      Assert.AreEqual(sizeof(sbyte), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertSByte((sbyte) (~expectedValue)));
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
    public void TestAssertInt16(Endianness endianness, byte[] bytes, short expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertInt16(expectedValue));
      Assert.AreEqual(sizeof(short), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertInt16((short) (~expectedValue)));
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
    public void TestAssertUInt16(Endianness endianness, byte[] bytes, ushort expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertUInt16(expectedValue));
      Assert.AreEqual(sizeof(ushort), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertUInt16((ushort) (~expectedValue)));
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
    public void TestAssertInt24(Endianness endianness, byte[] bytes, int expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertInt24(expectedValue));
      Assert.AreEqual(3, ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertInt24((int) (~expectedValue)));
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
    public void TestAssertUInt24(Endianness endianness, byte[] bytes, uint expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertUInt24(expectedValue));
      Assert.AreEqual(3, ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertUInt24((uint) (~expectedValue)));
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
    public void TestAssertInt32(Endianness endianness, byte[] bytes, int expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertInt32(expectedValue));
      Assert.AreEqual(sizeof(int), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertInt32((int) (~expectedValue)));
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
    public void TestAssertUInt32(Endianness endianness, byte[] bytes, uint expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertUInt32(expectedValue));
      Assert.AreEqual(sizeof(uint), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertUInt32((uint) (~expectedValue)));
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
    public void TestAssertInt64(Endianness endianness, byte[] bytes, long expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertInt64(expectedValue));
      Assert.AreEqual(sizeof(long), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertInt64((long) (~expectedValue)));
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
    public void TestAssertUInt64(Endianness endianness, byte[] bytes, ulong expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertUInt64(expectedValue));
      Assert.AreEqual(sizeof(ulong), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertUInt64((ulong) (~expectedValue)));
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x3C }, 1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x3C, 0x00 }, 1)]
    public void TestAssertHalf(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertHalf(expectedValue));
      Assert.AreEqual(2, ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertHalf(10 + expectedValue));
    }

    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x80, 0x3F }, 1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x3F, 0x80, 0x00, 0x00 }, 1)]
    public void TestAssertSingle(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertSingle(expectedValue));
      Assert.AreEqual(sizeof(float), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertSingle(10 + expectedValue));
    }

    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F }, 1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 1)]
    public void TestAssertDouble(Endianness endianness, byte[] bytes, double expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertDouble(expectedValue));
      Assert.AreEqual(sizeof(double), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertDouble(10 + expectedValue));
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
    public void TestAssertSn8(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertSn8(expectedValue));
      Assert.AreEqual(sizeof(sbyte), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertSn8(10 + expectedValue));
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
    public void TestAssertUn8(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertUn8(expectedValue));
      Assert.AreEqual(sizeof(byte), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertUn8(10 + expectedValue));
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
    public void TestAssertSn16(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertSn16(expectedValue));
      Assert.AreEqual(sizeof(short), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertSn16(10 + expectedValue));
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
    public void TestAssertUn16(Endianness endianness, byte[] bytes, float expectedValue) {
      using var ms = new MemoryStream(bytes);
      using var er = new EndianBinaryReader(ms, endianness);

      Assert.DoesNotThrow(() => er.AssertUn16(expectedValue));
      Assert.AreEqual(sizeof(ushort), ms.Position);

      er.Position = 0;
      Assert.Throws<Exception>(() => er.AssertUn16(10 + expectedValue));
    }
  }
}