using System;
using System.IO;

using NUnit.Framework;

namespace schema.util.streams {
  public class ReadableStreamTests {
    [Test]
    public void TestPosition() {
      var ms = new MemoryStream(new byte[] { 1, 2, 3 });
      var rs = new ReadableStream(ms);

      Assert.AreEqual(0, ms.Position);
      Assert.AreEqual(0, rs.Position);

      rs.Position = 2;
      Assert.AreEqual(2, ms.Position);
      Assert.AreEqual(2, rs.Position);
    }

    [Test]
    public void TestLength() {
      var ms = new MemoryStream(new byte[] { 1, 2, 3 });
      var rs = new ReadableStream(ms);

      Assert.AreEqual(3, ms.Length);
      Assert.AreEqual(3, rs.Length);
    }

    [Test]
    public void TestReadByte() {
      var ms = new MemoryStream(new byte[] { 1, 2, 3 });
      var rs = new ReadableStream(ms);

      Assert.AreEqual(0, ms.Position);
      Assert.AreEqual(0, rs.Position);

      Assert.AreEqual(1, rs.ReadByte());
      Assert.AreEqual(1, ms.Position);
      Assert.AreEqual(1, rs.Position);

      Assert.AreEqual(2, rs.ReadByte());
      Assert.AreEqual(2, ms.Position);
      Assert.AreEqual(2, rs.Position);
    }

    [Test]
    public void TestReadSpan() {
      var ms = new MemoryStream(new byte[] { 1, 2, 3 });
      var rs = new ReadableStream(ms);

      Assert.AreEqual(0, ms.Position);
      Assert.AreEqual(0, rs.Position);

      Span<byte> span = stackalloc byte[5];

      Assert.AreEqual(3, rs.Read(span));
      Assert.AreEqual(3, ms.Position);
      Assert.AreEqual(3, rs.Position);

      CollectionAssert.AreEqual(new[] { 1, 2, 3, 0, 0 }, span.ToArray());
    }
  }
}