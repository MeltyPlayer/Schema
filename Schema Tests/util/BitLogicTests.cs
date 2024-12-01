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
}