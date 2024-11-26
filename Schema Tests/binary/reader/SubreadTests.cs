using NUnit.Framework;

namespace schema.binary;

internal class SubreadTests {
  [Test]
  public void TestSubreadWithoutReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);

    br.Subread(
        50,
        sbr => {
          Assert.AreEqual(3, sbr.Position);
          Assert.AreEqual(53, sbr.Length);

          sbr.Position = 4;
          Assert.AreEqual(4, sbr.Position);
          Assert.AreEqual(53, sbr.Length);
        });

    Assert.AreEqual(53, br.Position);
    Assert.AreEqual(100, br.Length);
  }

  [Test]
  public void TestSubreadWithReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);

    var returnValue = br.Subread(
        50,
        sbr => {
          Assert.AreEqual(3, sbr.Position);
          Assert.AreEqual(53, sbr.Length);

          sbr.Position = 4;
          Assert.AreEqual(4, sbr.Position);
          Assert.AreEqual(53, sbr.Length);

          return 123;
        });
    Assert.AreEqual(123, returnValue);

    Assert.AreEqual(53, br.Position);
    Assert.AreEqual(100, br.Length);
  }

  [Test]
  public void TestSubreadAtWithoutLengthWithoutReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);

    br.SubreadAt(
        4,
        sbr => {
          Assert.AreEqual(4, sbr.Position);
          Assert.AreEqual(100, sbr.Length);
        });

    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);
  }

  [Test]
  public void TestSubreadAtWithLengthWithoutReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);

    br.SubreadAt(
        4,
        50,
        sbr => {
          Assert.AreEqual(4, sbr.Position);
          Assert.AreEqual(54, sbr.Length);
        });

    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);
  }

  [Test]
  public void TestSubreadAtWithoutLengthWithReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);

    var returnValue = br.SubreadAt(
        4,
        sbr => {
          Assert.AreEqual(4, sbr.Position);
          Assert.AreEqual(100, sbr.Length);
          return 123;
        });
    Assert.AreEqual(123, returnValue);

    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);
  }

  [Test]
  public void TestSubreadAtWithLengthWithReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);

    var returnValue = br.SubreadAt(
        4,
        50,
        sbr => {
          Assert.AreEqual(4, sbr.Position);
          Assert.AreEqual(54, sbr.Length);
          return 123;
        });
    Assert.AreEqual(123, returnValue);

    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(100, br.Length);
  }
}