using System;
using System.Numerics;

using NUnit.Framework;


namespace schema.binary;

public class SchemaBinaryReaderNumbersTests {
  delegate void ReadSpan<T>(Span<T> span);

  private void AssertEquals_<T>(ReadOnlySpan<T> expectedValues,
                                ReadOnlySpan<T> actualValues)
      where T : INumber<T> {
    Assert.AreEqual(expectedValues.Length, actualValues.Length);
    for (var i = 0; i < expectedValues.Length; ++i) {
      var expectedValue = expectedValues[i];
      var actualValue = actualValues[i];

      if (expectedValue is float expectedSingle &&
          actualValue is float actualSingle) {
        Assert.AreEqual(expectedSingle, actualSingle, .01f);
      } else if (expectedValue is double expectedDouble &&
                 actualValue is double actualDouble) {
        Assert.AreEqual(expectedDouble, actualDouble, .01);
      } else {
        Assert.AreEqual(expectedValue, actualValue);
      }
    }
  }

  private void ReadAndAssert_<T>(
      IBinaryReader br,
      T[] expectedValues,
      Func<long, T[]> readHandler,
      ReadSpan<T> readSpanHandler) where T : unmanaged, INumber<T> {
    var readValues = readHandler(expectedValues.Length);
    this.AssertEquals_<T>(expectedValues, readValues);

    br.Position = 0;
    Span<T> readIntoSpan = stackalloc T[expectedValues.Length];
    readSpanHandler(readIntoSpan);
    this.AssertEquals_<T>(expectedValues, readIntoSpan);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] { 0x00, 0xFF, 0x7F, 0x80 },
            new byte[] { byte.MinValue, byte.MaxValue, 127, 128 })]
  [TestCase(Endianness.BigEndian,
            new byte[] { 0x00, 0xFF, 0x7F, 0x80 },
            new byte[] { byte.MinValue, byte.MaxValue, 127, 128 })]
  public void TestReadAndAssertBytes(Endianness endianness,
                                     byte[] bytes,
                                     byte[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadBytes,
                        br.ReadBytes);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] { 0x00, 0xFF, 0x7F, 0x80 },
            new sbyte[] { 0, -1, sbyte.MaxValue, sbyte.MinValue })]
  [TestCase(Endianness.BigEndian,
            new byte[] { 0x00, 0xFF, 0x7F, 0x80 },
            new sbyte[] { 0, -1, sbyte.MaxValue, sbyte.MinValue })]
  public void TestReadAndAssertSBytes(Endianness endianness,
                                      byte[] bytes,
                                      sbyte[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadSBytes,
                        br.ReadSBytes);
  }


  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] { 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x7F, 0x00, 0x80 },
            new short[] { 0, -1, short.MaxValue, short.MinValue })]
  [TestCase(Endianness.BigEndian,
            new byte[] { 0x00, 0x00, 0xFF, 0xFF, 0x7F, 0xFF, 0x80, 0x00 },
            new short[] { 0, -1, short.MaxValue, short.MinValue })]
  public void TestReadAndAssertInt16s(Endianness endianness,
                                      byte[] bytes,
                                      short[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadInt16s,
                        br.ReadInt16s);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] { 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x7F, 0x00, 0x80 },
            new ushort[] { ushort.MinValue, ushort.MaxValue, 32767, 32768 })]
  [TestCase(Endianness.BigEndian,
            new byte[] { 0x00, 0x00, 0xFF, 0xFF, 0x7F, 0xFF, 0x80, 0x00 },
            new ushort[] { ushort.MinValue, ushort.MaxValue, 32767, 32768 })]
  public void TestReadAndAssertUInt16s(Endianness endianness,
                                       byte[] bytes,
                                       ushort[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadUInt16s,
                        br.ReadUInt16s);
  }


  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0x00,
                0x00,
                0x80,
            },
            new[] { 0, -1, 8388607, -8388608 })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0xFF,
                0xFF,
                0x80,
                0x00,
                0x00,
            },
            new[] { 0, -1, 8388607, -8388608 })]
  public void TestReadAndAssertInt24s(Endianness endianness,
                                      byte[] bytes,
                                      int[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadInt24s,
                        br.ReadInt24s);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0x00,
                0x00,
                0x80,
            },
            new uint[] { 0, 16777215, 8388607, 8388608 })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0xFF,
                0xFF,
                0x80,
                0x00,
                0x00,
            },
            new uint[] { 0, 16777215, 8388607, 8388608 })]
  public void TestReadAndAssertUInt24s(Endianness endianness,
                                       byte[] bytes,
                                       uint[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadUInt24s,
                        br.ReadUInt24s);
  }


  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0x00,
                0x00,
                0x00,
                0x80,
            },
            new[] { 0, -1, int.MaxValue, int.MinValue })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0xFF,
                0xFF,
                0xFF,
                0x80,
                0x00,
                0x00,
                0x00,
            },
            new[] { 0, -1, int.MaxValue, int.MinValue })]
  public void TestReadAndAssertInt32s(Endianness endianness,
                                      byte[] bytes,
                                      int[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadInt32s,
                        br.ReadInt32s);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0x00,
                0x00,
                0x00,
                0x80,
            },
            new uint[] {
                uint.MinValue, uint.MaxValue, 2147483647, 2147483648
            })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0xFF,
                0xFF,
                0xFF,
                0x80,
                0x00,
                0x00,
                0x00,
            },
            new uint[] {
                uint.MinValue, uint.MaxValue, 2147483647, 2147483648
            })]
  public void TestReadAndAssertUInt32(Endianness endianness,
                                      byte[] bytes,
                                      uint[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadUInt32s,
                        br.ReadUInt32s);
  }


  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x80,
            },
            new[] { 0, -1, long.MaxValue, long.MinValue })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x80,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            },
            new[] { 0, -1, long.MaxValue, long.MinValue })]
  public void TestReadAndAssertInt64s(Endianness endianness,
                                      byte[] bytes,
                                      long[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadInt64s,
                        br.ReadInt64s);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x80,
            },
            new ulong[] {
                ulong.MinValue,
                ulong.MaxValue,
                9223372036854775807,
                9223372036854775808
            })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x7F,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0xFF,
                0x80,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            },
            new ulong[] {
                ulong.MinValue,
                ulong.MaxValue,
                9223372036854775807,
                9223372036854775808
            })]
  public void TestReadAndAssertUInt64s(Endianness endianness,
                                       byte[] bytes,
                                       ulong[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadUInt64s,
                        br.ReadUInt64s);
  }


  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] { 0x00, 0x00, 0x00, 0x3C },
            new float[] { 0, 1 })]
  [TestCase(Endianness.BigEndian,
            new byte[] { 0x00, 0x00, 0x3C, 0x00 },
            new float[] { 0, 1 })]
  public void TestReadAndAssertHalfs(Endianness endianness,
                                     byte[] bytes,
                                     float[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadHalfs,
                        br.ReadHalfs);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, },
            new float[] { 0, 1 })]
  [TestCase(Endianness.BigEndian,
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, },
            new float[] { 0, 1 })]
  public void TestReadAndAssertSingles(Endianness endianness,
                                       byte[] bytes,
                                       float[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadSingles,
                        br.ReadSingles);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0xF0,
                0x3F
            },
            new double[] { 0, 1 })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x3F,
                0xF0,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00
            },
            new double[] { 0, 1 })]
  public void TestReadAndAssertDoubles(Endianness endianness,
                                       byte[] bytes,
                                       double[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadDoubles,
                        br.ReadDoubles);
  }


  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00, 0x20, 0x40, 0x60, 0x7F, 0x80, 0xA0, 0xC0, 0xE0, 0xFF
            },
            new[] { 0, .25f, .5f, .75f, 1, -1, -.75f, -.5f, -.25f, 0, })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00, 0x20, 0x40, 0x60, 0x7F, 0x80, 0xA0, 0xC0, 0xE0, 0xFF
            },
            new[] { 0, .25f, .5f, .75f, 1, -1, -.75f, -.5f, -.25f, 0, })]
  public void TestReadAndAssertSn8s(Endianness endianness,
                                    byte[] bytes,
                                    float[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadSn8s,
                        br.ReadSn8s);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] { 0x00, 0x40, 0x80, 0xC0, 0xFF },
            new[] { 0, .25f, .5f, .75f, 1 })]
  [TestCase(Endianness.BigEndian,
            new byte[] { 0x00, 0x40, 0x80, 0xC0, 0xFF },
            new[] { 0, .25f, .5f, .75f, 1 })]
  public void TestReadAndAssertUn8s(Endianness endianness,
                                    byte[] bytes,
                                    float[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadUn8s,
                        br.ReadUn8s);
  }


  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00,
                0x00,
                0x00,
                0x20,
                0x00,
                0x40,
                0x00,
                0x60,
                0xFF,
                0x7F,
                0x00,
                0x80,
                0x00,
                0xA0,
                0x00,
                0xC0,
                0x00,
                0xE0,
                0xFF,
                0xFF,
            },
            new[] { 0, .25f, .5f, .75f, 1, -1, -.75f, -.5f, -.25f, 0, })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00,
                0x00,
                0x20,
                0x00,
                0x40,
                0x00,
                0x60,
                0x00,
                0x7F,
                0xFF,
                0x80,
                0x00,
                0xA0,
                0x00,
                0xC0,
                0x00,
                0xE0,
                0x00,
                0xFF,
                0xFF,
            },
            new[] { 0, .25f, .5f, .75f, 1, -1, -.75f, -.5f, -.25f, 0, })]
  public void TestReadAndAssertSn16s(Endianness endianness,
                                     byte[] bytes,
                                     float[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadSn16s,
                        br.ReadSn16s);
  }

  [Test]
  [TestCase(Endianness.LittleEndian,
            new byte[] {
                0x00, 0x00, 0x00, 0x40, 0x00, 0x80, 0x00, 0xC0, 0xFF, 0xFF,
            },
            new[] { 0, .25f, .5f, .75f, 1 })]
  [TestCase(Endianness.BigEndian,
            new byte[] {
                0x00, 0x00, 0x40, 0x00, 0x80, 0x00, 0xC0, 0x00, 0xFF, 0xFF,
            },
            new[] { 0, .25f, .5f, .75f, 1 })]
  public void TestReadAndAssertUn16s(Endianness endianness,
                                     byte[] bytes,
                                     float[] expectedValues) {
    using var br = new SchemaBinaryReader(bytes, endianness);
    this.ReadAndAssert_(br,
                        expectedValues,
                        br.ReadUn16s,
                        br.ReadUn16s);
  }
}