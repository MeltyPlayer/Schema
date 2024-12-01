using NUnit.Framework;


namespace schema.util;

public class BitLogicTests {
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
}