using NUnit.Framework;


namespace schema.util;

public class BitLogicTests {
  [Test]
  [TestCase(0, ExpectedResult = (uint) 0)]
  [TestCase(1, ExpectedResult = (uint) 0b1)]
  [TestCase(2, ExpectedResult = (uint) 0b11)]
  [TestCase(3, ExpectedResult = (uint) 0b111)]
  [TestCase(4, ExpectedResult = (uint) 0b1111)]
  public uint TestCreateMask(int bits)
    => BitLogic.CreateMask(bits);

  [Test]
  [TestCase((uint) 0, ExpectedResult = (uint) 0)]
  [TestCase((uint) 1, ExpectedResult = (uint) 1)]
  [TestCase((uint) 8, ExpectedResult = (uint) 1)]
  [TestCase((uint) 9, ExpectedResult = (uint) 2)]
  [TestCase((uint) 16, ExpectedResult = (uint) 2)]
  [TestCase((uint) 17, ExpectedResult = (uint) 3)]
  public uint TestBytesNeededToContainBits(uint bits)
    => BitLogic.BytesNeededToContainBits(bits);

  [Test]
  [TestCase((uint) 0, ExpectedResult = 0.0f)]
  [TestCase((uint) 1, ExpectedResult = 0.000244140625f)]
  [TestCase((uint) 100, ExpectedResult = 0.0244140625f)]
  [TestCase(0xF0000000, ExpectedResult = -65536f)]
  [TestCase(uint.MaxValue, ExpectedResult = -0.000244140625f)]
  public float TestFixedPointToSingle_1_19_12(uint x)
    => BitLogic.ConvertFixedPointToSingle(x, 1, 19, 12);

  [Test]
  [TestCase((uint) 0, ExpectedResult = 0.0)]
  [TestCase((uint) 1, ExpectedResult = 0.000244140625)]
  [TestCase((uint) 100, ExpectedResult = 0.0244140625)]
  [TestCase(0xF0000000, ExpectedResult = -65536)]
  [TestCase(uint.MaxValue, ExpectedResult = -0.000244140625)]
  public double TestFixedPointToDouble_1_19_12(uint x)
    => BitLogic.ConvertFixedPointToDouble(x, 1, 19, 12);

  [Test]
  [TestCase(0.0f, ExpectedResult = (uint) 0)]
  [TestCase(0.000244140625f, ExpectedResult = (uint) 1)]
  [TestCase(0.0244140625f, ExpectedResult = (uint) 100)]
  [TestCase(-65536f, ExpectedResult = 0xF0000000)]
  [TestCase(-0.000244140625f, ExpectedResult = uint.MaxValue)]
  public uint TestSingleToFixedPoint_1_19_12(float x)
    => BitLogic.ConvertSingleToFixedPoint(x, 1, 19, 12);

  [Test]
  [TestCase(0.0, ExpectedResult = (uint) 0)]
  [TestCase(0.000244140625, ExpectedResult = (uint) 1)]
  [TestCase(0.0244140625, ExpectedResult = (uint) 100)]
  [TestCase(-65536, ExpectedResult = 0xF0000000)]
  [TestCase(-0.000244140625, ExpectedResult = uint.MaxValue)]
  public uint TestDoubleToFixedPoint_1_19_12(double x)
    => BitLogic.ConvertDoubleToFixedPoint(x, 1, 19, 12);

  [Test]
  [TestCase((uint) 0, ExpectedResult = 0.0f)]
  [TestCase((uint) 1, ExpectedResult = 0.00390625f)]
  [TestCase((uint) 100, ExpectedResult = 0.390625f)]
  [TestCase((uint) 0xF000, ExpectedResult = -16f)]
  [TestCase(uint.MaxValue, ExpectedResult = -0.00390625f)]
  public float TestFixedPointToSingle_1_7_8(uint x)
    => BitLogic.ConvertFixedPointToSingle(x, 1, 7, 8);

  [Test]
  [TestCase((uint) 0, ExpectedResult = 0.0)]
  [TestCase((uint) 1, ExpectedResult = 0.00390625d)]
  [TestCase((uint) 100, ExpectedResult = 0.390625d)]
  [TestCase((uint) 0xF000, ExpectedResult = -16)]
  [TestCase(uint.MaxValue, ExpectedResult = -0.00390625)]
  public double TestFixedPointToDouble_1_7_8(uint x)
    => BitLogic.ConvertFixedPointToDouble(x, 1, 7, 8);

  [Test]
  [TestCase(0.0f, ExpectedResult = (uint) 0)]
  [TestCase(0.00390625f, ExpectedResult = (uint) 1)]
  [TestCase(0.390625f, ExpectedResult = (uint) 100)]
  [TestCase(-16f, ExpectedResult = (uint) 0xF000)]
  [TestCase(-0.00390625f, ExpectedResult = ushort.MaxValue)]
  public uint TestSingleToFixedPoint_1_7_8(float x)
    => BitLogic.ConvertSingleToFixedPoint(x, 1, 7, 8);

  [Test]
  [TestCase(0.0, ExpectedResult = (uint) 0)]
  [TestCase(0.00390625d, ExpectedResult = (uint) 1)]
  [TestCase(0.390625d, ExpectedResult = (uint) 100)]
  [TestCase(-16, ExpectedResult = (uint) 0xF000)]
  [TestCase(-0.00390625, ExpectedResult = ushort.MaxValue)]
  public uint TestDoubleToFixedPoint_1_7_8(double x)
    => BitLogic.ConvertDoubleToFixedPoint(x, 1, 7, 8);
}