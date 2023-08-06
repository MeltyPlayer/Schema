using System.IO;

using NUnit.Framework;

namespace schema.binary {
  public class EndianBinaryReaderPositionTests {
    [Test]
    public void TestNestedSpaces() {
      var data = new byte[100];
      var ms = new MemoryStream(data);
      var er = new EndianBinaryReader(ms);
      Assert.AreEqual(0, er.Position);
      Assert.AreEqual(100, er.Length);

      er.Position = 5;
      Assert.AreEqual(5, er.Position);
      Assert.AreEqual(100, er.Length);

      er.PushLocalSpace();
      {
        Assert.AreEqual(0, er.Position);
        Assert.AreEqual(95, er.Length);

        er.Position = 5;
        Assert.AreEqual(5, er.Position);
        Assert.AreEqual(95, er.Length);

        er.PushLocalSpace();
        {
          Assert.AreEqual(0, er.Position);
          Assert.AreEqual(90, er.Length);

          er.Position = 5;
          Assert.AreEqual(5, er.Position);
          Assert.AreEqual(90, er.Length);
        }
        er.PopLocalSpace();
        Assert.AreEqual(10, er.Position);
        Assert.AreEqual(95, er.Length);
      }
      er.PopLocalSpace();
      Assert.AreEqual(15, er.Position);
      Assert.AreEqual(100, er.Length);
    }

    [Test]
    public void TestNestedPointer() {
      var data = new byte[100];
      var ms = new MemoryStream(data);
      var er = new EndianBinaryReader(ms);
      Assert.AreEqual(0, er.Position);

      er.Position = 5;
      Assert.AreEqual(5, er.Position);

      er.PushLocalSpace();
      {
        Assert.AreEqual(0, er.Position);

        er.Position = 3;
        Assert.AreEqual(3, er.Position);

        er.Subread(
            3,
            50,
            ser => {
              Assert.AreEqual(3, ser.Position);
            });
      }
    }
  }
}