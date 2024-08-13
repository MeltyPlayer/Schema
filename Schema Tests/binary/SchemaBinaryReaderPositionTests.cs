using System.IO;

using NUnit.Framework;


namespace schema.binary;

public class SchemaBinaryReaderPositionTests {
  [Test]
  public void TestNestedSpaces() {
      var data = new byte[100];
      var ms = new MemoryStream(data);
      using var br = new SchemaBinaryReader(ms);
      Assert.AreEqual(0, br.Position);
      Assert.AreEqual(100, br.Length);

      br.Position = 5;
      Assert.AreEqual(5, br.Position);
      Assert.AreEqual(100, br.Length);

      br.PushLocalSpace();
      {
        Assert.AreEqual(0, br.Position);
        Assert.AreEqual(95, br.Length);

        br.Position = 5;
        Assert.AreEqual(5, br.Position);
        Assert.AreEqual(95, br.Length);

        br.PushLocalSpace();
        {
          Assert.AreEqual(0, br.Position);
          Assert.AreEqual(90, br.Length);

          br.Position = 5;
          Assert.AreEqual(5, br.Position);
          Assert.AreEqual(90, br.Length);
        }
        br.PopLocalSpace();
        Assert.AreEqual(10, br.Position);
        Assert.AreEqual(95, br.Length);
      }
      br.PopLocalSpace();
      Assert.AreEqual(15, br.Position);
      Assert.AreEqual(100, br.Length);
    }

  [Test]
  public void TestNestedPointer() {
      var data = new byte[100];
      var ms = new MemoryStream(data);
      using var br = new SchemaBinaryReader(ms);
      Assert.AreEqual(0, br.Position);
      Assert.AreEqual(100, br.Length);

      br.Position = 5;
      Assert.AreEqual(5, br.Position);
      Assert.AreEqual(100, br.Length);

      br.PushLocalSpace();
      {
        Assert.AreEqual(0, br.Position);
        Assert.AreEqual(95, br.Length);

        br.Position = 3;
        Assert.AreEqual(3, br.Position);
        Assert.AreEqual(95, br.Length);

        br.SubreadAt(
            3,
            50,
            sbr => {
              Assert.AreEqual(3, sbr.Position);
              Assert.AreEqual(53, sbr.Length);
            });
      }
      br.PopLocalSpace();

      Assert.AreEqual(8, br.Position);
      Assert.AreEqual(100, br.Length);
    }

  [Test]
  public void TestAlign() {
      using var br = new SchemaBinaryReader(new MemoryStream(100));
      Assert.AreEqual(0, br.Position);

      br.Align(4);
      Assert.AreEqual(0, br.Position);

      br.Position = 1;
      br.Align(4);
      Assert.AreEqual(4, br.Position);
    }

  [Test]
  public void TestAssertPosition() {
      using var br = new SchemaBinaryReader(new MemoryStream(5));

      br.Position = 0;
      Assert.That(() => br.AssertPosition(5), Throws.Exception);

      br.Position = 5;
      Assert.That(() => br.AssertPosition(5), Throws.Nothing);
    }

  [Test]
  public void TestAssertEofThrowsPastEnd() {
      using var br = new SchemaBinaryReader(new MemoryStream());
      Assert.That(br.AssertNotEof, Throws.Exception);
    }
}