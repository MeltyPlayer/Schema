using System;

using NUnit.Framework;

namespace schema.binary {
  public class SchemaBinaryReaderEndiannessTests {
    [Test]
    [TestCase(Endianness.BigEndian)]
    [TestCase(Endianness.LittleEndian)]
    public void TestInitialEndianness(Endianness endianness) {
      var br = new SchemaBinaryReader(Array.Empty<byte>(), endianness);
      Assert.AreEqual(endianness, br.Endianness);
    }

    [Test]
    public void TestInitialSystemEndianness() {
      var br = new SchemaBinaryReader(Array.Empty<byte>());
      Assert.AreEqual(EndiannessUtil.SystemEndianness, br.Endianness);
    }

    [Test]
    public void TestOpposite() {
      var br = new SchemaBinaryReader(
          Array.Empty<byte>(),
          EndiannessUtil.SystemEndianness.GetOpposite());
      Assert.True(br.IsOppositeEndiannessOfSystem);
    }

    [Test]
    [TestCase(Endianness.BigEndian)]
    [TestCase(Endianness.LittleEndian)]
    public void TestPushContainerEndianness(Endianness endianness) {
      var br = new SchemaBinaryReader(Array.Empty<byte>(),
                                      endianness.GetOpposite());
      br.PushContainerEndianness(endianness);
      Assert.AreEqual(endianness, br.Endianness);
    }

    [Test]
    [TestCase(Endianness.BigEndian)]
    [TestCase(Endianness.LittleEndian)]
    public void TestPushMemberEndianness(Endianness endianness) {
      var br = new SchemaBinaryReader(Array.Empty<byte>(),
                                      endianness.GetOpposite());
      br.PushMemberEndianness(endianness);
      Assert.AreEqual(endianness, br.Endianness);
    }

    [Test]
    [TestCase(Endianness.BigEndian)]
    [TestCase(Endianness.LittleEndian)]
    public void TestPopEndianness(Endianness endianness) {
      var br = new SchemaBinaryReader(Array.Empty<byte>(),
                                      endianness.GetOpposite());

      br.PushMemberEndianness(endianness);
      Assert.AreEqual(endianness, br.Endianness);

      br.PopEndianness();
      Assert.AreEqual(endianness.GetOpposite(), br.Endianness);
    }
  }
}