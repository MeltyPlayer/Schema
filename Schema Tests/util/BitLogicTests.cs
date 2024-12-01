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
  [TestCase((uint) 0, ExpectedResult = 0.0)]
  [TestCase((uint) 1, ExpectedResult = 0.000244140625)]
  [TestCase((uint) 100, ExpectedResult = 0.0244140625)]
  [TestCase(0xF0000000, ExpectedResult = -65536)]
  [TestCase(uint.MaxValue, ExpectedResult = -0.000244140625)]
  public double TestFixed32_1_19_12(uint x)
    => BitLogic.GetFixedPointDouble(x, 1, 19, 12);
}