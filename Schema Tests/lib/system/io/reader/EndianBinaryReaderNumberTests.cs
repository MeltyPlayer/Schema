using System;
using System.Numerics;

using NUnit.Framework;

namespace schema.binary {
  public class EndianBinaryReaderNumberTests {
    private void ReadAndAssert_<T>(
        IEndianBinaryReader er,
        T expectedValue,
        Func<T> readHandler,
        Action<T> assertValue) where T : INumber<T> {
      var actualValue = readHandler();
      if (expectedValue is float expectedSingle &&
          actualValue is float actualSingle) {
        Assert.AreEqual(expectedSingle, actualSingle, .01f);
      }
      else if (expectedValue is double expectedDouble &&
          actualValue is double actualDouble) {
        Assert.AreEqual(expectedDouble, actualDouble, .01);
      } else {
        Assert.AreEqual(expectedValue, actualValue);
      }

      er.Position = 0;
      assertValue(expectedValue);
    }

    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00 }, byte.MinValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF }, byte.MaxValue)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x7F }, (byte) 127)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x80 }, (byte) 128)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00 }, byte.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF }, byte.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F }, (byte) 127)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80 }, (byte) 128)]
    public void TestReadAndAssertByte(Endianness endianness,
                                      byte[] bytes,
                                      byte expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadByte, er.AssertByte);
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
    public void TestReadAndAssertSByte(Endianness endianness,
                                       byte[] bytes,
                                       sbyte expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadSByte, er.AssertSByte);
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0x7F },
              short.MaxValue)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x80 },
              short.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF }, short.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00 }, short.MinValue)]
    public void TestReadAndAssertInt16(Endianness endianness,
                                       byte[] bytes,
                                       short expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadInt16, er.AssertInt16);
    }

    [Test]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00 },
              ushort.MinValue)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF },
              ushort.MaxValue)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0x7F },
              (ushort) 32767)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x80 },
              (ushort) 32768)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00 }, ushort.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF }, ushort.MaxValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF }, (ushort) 32767)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00 }, (ushort) 32768)]
    public void TestReadAndAssertUInt16(Endianness endianness,
                                        byte[] bytes,
                                        ushort expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadUInt16, er.AssertUInt16);
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0x7F },
              8388607)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x80 },
              -8388608)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x7F, 0xFF, 0xFF }, 8388607)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x80, 0x00, 0x00 }, -8388608)]
    public void TestReadAndAssertInt24(Endianness endianness,
                                       byte[] bytes,
                                       int expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadInt24, er.AssertInt24);
    }

    [Test]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00 },
              (uint) 0)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0xFF },
              (uint) 16777215)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0x7F },
              (uint) 8388607)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x80 },
              (uint) 8388608)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00 }, (uint) 0)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0xFF, 0xFF, 0xFF },
              (uint) 16777215)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x7F, 0xFF, 0xFF },
              (uint) 8388607)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x80, 0x00, 0x00 },
              (uint) 8388608)]
    public void TestReadAndAssertUInt24(Endianness endianness,
                                        byte[] bytes,
                                        uint expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadUInt24, er.AssertUInt24);
    }


    [Test]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00 },
              0)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0xFF },
              -1)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0x7F },
              int.MaxValue)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x80 },
              int.MinValue)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, -1)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x7F, 0xFF, 0xFF, 0xFF },
              int.MaxValue)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x80, 0x00, 0x00, 0x00 },
              int.MinValue)]
    public void TestReadAndAssertInt32(Endianness endianness,
                                       byte[] bytes,
                                       int expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadInt32, er.AssertInt32);
    }

    [Test]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00 },
              uint.MinValue)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0xFF },
              uint.MaxValue)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0x7F },
              (uint) 2147483647)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x80 },
              (uint) 2147483648)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00 },
              uint.MinValue)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0xFF },
              uint.MaxValue)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x7F, 0xFF, 0xFF, 0xFF },
              (uint) 2147483647)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x80, 0x00, 0x00, 0x00 },
              (uint) 2147483648)]
    public void TestReadAndAssertUInt32(Endianness endianness,
                                        byte[] bytes,
                                        uint expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadUInt32, er.AssertUInt32);
    }


    [Test]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
              0)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
              -1)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F },
              long.MaxValue)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
              long.MinValue)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
              0)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
              -1)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
              long.MaxValue)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
              long.MinValue)]
    public void TestReadAndAssertInt64(Endianness endianness,
                                       byte[] bytes,
                                       long expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadInt64, er.AssertInt64);
    }

    [Test]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
              ulong.MinValue)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
              ulong.MaxValue)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F },
              (ulong) 9223372036854775807)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 },
              (ulong) 9223372036854775808)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
              ulong.MinValue)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
              ulong.MaxValue)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
              (ulong) 9223372036854775807)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
              (ulong) 9223372036854775808)]
    public void TestReadAndAssertUInt64(Endianness endianness,
                                        byte[] bytes,
                                        ulong expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadUInt64, er.AssertUInt64);
    }


    [Test]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x00 }, 0)]
    [TestCase(Endianness.LittleEndian, new byte[] { 0x00, 0x3C }, 1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x3C, 0x00 }, 1)]
    public void TestReadAndAssertHalf(Endianness endianness,
                                      byte[] bytes,
                                      float expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadHalf, er.AssertHalf);
    }

    [Test]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00 },
              0)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x80, 0x3F },
              1)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0)]
    [TestCase(Endianness.BigEndian, new byte[] { 0x3F, 0x80, 0x00, 0x00 }, 1)]
    public void TestReadAndAssertSingle(Endianness endianness,
                                        byte[] bytes,
                                        float expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadSingle, er.AssertSingle);
    }

    [Test]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
              0)]
    [TestCase(Endianness.LittleEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F },
              1)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
              0)]
    [TestCase(Endianness.BigEndian,
              new byte[] { 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
              1)]
    public void TestReadAndAssertDouble(Endianness endianness,
                                        byte[] bytes,
                                        double expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadDouble, er.AssertDouble);
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
    public void TestReadAndAssertSn8(Endianness endianness,
                                     byte[] bytes,
                                     float expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadSn8, er.AssertSn8);
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
    public void TestReadAndAssertUn8(Endianness endianness,
                                     byte[] bytes,
                                     float expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadUn8, er.AssertUn8);
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
    public void TestReadAndAssertSn16(Endianness endianness,
                                      byte[] bytes,
                                      float expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadSn16, er.AssertSn16);
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
    public void TestReadAndAssertUn16(Endianness endianness,
                                      byte[] bytes,
                                      float expectedValue) {
      using var er = new EndianBinaryReader(bytes, endianness);
      this.ReadAndAssert_(er, expectedValue, er.ReadUn16, er.AssertUn16);
    }
  }
}