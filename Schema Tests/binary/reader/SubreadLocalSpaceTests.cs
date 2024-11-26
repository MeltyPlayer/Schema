using NUnit.Framework;

namespace schema.binary;

internal class SubreadLocalSpaceTests {
  [Test]
  public void TestSubreadWithoutReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 2;
    br.PushLocalSpace();

    Assert.AreEqual(0, br.Position);
    Assert.AreEqual(98, br.Length);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);

    br.Subread(
        50,
        () => {
          Assert.AreEqual(3, br.Position);
          Assert.AreEqual(53, br.Length);

          br.Position = 4;
          Assert.AreEqual(4, br.Position);
          Assert.AreEqual(53, br.Length);
        });

    Assert.AreEqual(53, br.Position);
    Assert.AreEqual(98, br.Length);
  }

  [Test]
  public void TestSubreadWithReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 2;
    br.PushLocalSpace();

    Assert.AreEqual(0, br.Position);
    Assert.AreEqual(98, br.Length);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);

    var returnValue = br.Subread(
        50,
        () => {
          Assert.AreEqual(3, br.Position);
          Assert.AreEqual(53, br.Length);

          br.Position = 4;
          Assert.AreEqual(4, br.Position);
          Assert.AreEqual(53, br.Length);

          return 123;
        });
    Assert.AreEqual(123, returnValue);

    Assert.AreEqual(53, br.Position);
    Assert.AreEqual(98, br.Length);
  }

  [Test]
  public void TestSubreadAtWithoutLengthWithoutReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 2;
    br.PushLocalSpace();

    Assert.AreEqual(0, br.Position);
    Assert.AreEqual(98, br.Length);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);

    br.SubreadAt(
        4,
        () => {
          Assert.AreEqual(4, br.Position);
          Assert.AreEqual(98, br.Length);
        });

    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(98, br.Length);
  }

  [Test]
  public void TestSubreadAtWithLengthWithoutReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 2;
    br.PushLocalSpace();

    Assert.AreEqual(0, br.Position);
    Assert.AreEqual(98, br.Length);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);

    br.SubreadAt(
        4,
        50,
        () => {
          Assert.AreEqual(4, br.Position);
          Assert.AreEqual(54, br.Length);
        });

    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(98, br.Length);
  }

  [Test]
  public void TestSubreadAtWithoutLengthWithReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 2;
    br.PushLocalSpace();

    Assert.AreEqual(0, br.Position);
    Assert.AreEqual(98, br.Length);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);

    var returnValue = br.SubreadAt(
        4,
        () => {
          Assert.AreEqual(4, br.Position);
          Assert.AreEqual(98, br.Length);
          return 123;
        });
    Assert.AreEqual(123, returnValue);

    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(98, br.Length);
  }

  [Test]
  public void TestSubreadAtWithLengthWithReturn() {
    using var br = new SchemaBinaryReader(new byte[100]);

    br.Position = 2;
    br.PushLocalSpace();

    Assert.AreEqual(0, br.Position);
    Assert.AreEqual(98, br.Length);

    br.Position = 3;
    Assert.AreEqual(3, br.Position);

    var returnValue = br.SubreadAt(
        4,
        50,
        () => {
          Assert.AreEqual(4, br.Position);
          Assert.AreEqual(54, br.Length);
          return 123;
        });
    Assert.AreEqual(123, returnValue);

    Assert.AreEqual(3, br.Position);
    Assert.AreEqual(98, br.Length);
  }
}