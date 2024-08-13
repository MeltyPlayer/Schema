using System;
using System.IO;
using System.Linq;

using NUnit.Framework;


namespace schema.util.streams;

public class WritableStreamTests {
  [Test]
  public void TestPosition() {
      var ms = new MemoryStream(new byte[] {1, 2, 3});
      var ws = new WritableStream(ms);

      Assert.AreEqual(0, ms.Position);
      Assert.AreEqual(0, ws.Position);

      ws.Position = 2;
      Assert.AreEqual(2, ms.Position);
      Assert.AreEqual(2, ws.Position);
    }

  [Test]
  public void TestLength() {
      var ms = new MemoryStream(new byte[] {1, 2, 3});
      var ws = new WritableStream(ms);

      Assert.AreEqual(3, ms.Length);
      Assert.AreEqual(3, ws.Length);
    }

  [Test]
  public void TestWriteByte() {
      var data = new byte[] {1, 2, 3};
      var ms = new MemoryStream(data);
      var ws = new WritableStream(ms);

      Assert.AreEqual(0, ms.Position);
      Assert.AreEqual(0, ws.Position);

      ws.WriteByte(5);
      Assert.AreEqual(5, data[0]);
      Assert.AreEqual(1, ms.Position);
      Assert.AreEqual(1, ws.Position);

      ws.WriteByte(6);
      Assert.AreEqual(6, data[1]);
      Assert.AreEqual(2, ms.Position);
      Assert.AreEqual(2, ws.Position);
    }

  [Test]
  public void TestWriteSpan() {
      var ms = new MemoryStream();
      var ws = new WritableStream(ms);

      Assert.AreEqual(0, ms.Position);
      Assert.AreEqual(0, ws.Position);

      ReadOnlySpan<byte> span = stackalloc byte[5] {5, 6, 7, 8, 9};

      ws.Write(span);
      Assert.AreEqual(5, ms.Position);
      Assert.AreEqual(5, ws.Position);

      CollectionAssert.AreEqual(new[] {5, 6, 7, 8, 9}, ms.ToArray());
    }

  [Test]
  public void TestWriteReadableStream() {
      var ms = new MemoryStream();
      var ws = new WritableStream(ms);
      var rs = new ReadableStream(new byte[] {5, 6, 7, 8, 9});

      Assert.AreEqual(0, ms.Position);
      Assert.AreEqual(0, ws.Position);
      Assert.AreEqual(0, rs.Position);

      ws.Write(rs);
      Assert.AreEqual(rs.Length, ms.Position);
      Assert.AreEqual(rs.Length, ws.Position);
      Assert.AreEqual(rs.Length, rs.Position);

      CollectionAssert.AreEqual(new[] {5, 6, 7, 8, 9}, ms.ToArray());
    }

  [Test]
  public void TestWriteLongReadableStream() {
      var readData =
          Enumerable.Range(0, 300_000).Select(i => (byte) i).ToArray();

      var ms = new MemoryStream();
      var ws = new WritableStream(ms);
      var rs = new ReadableStream(readData);

      Assert.AreEqual(0, ms.Position);
      Assert.AreEqual(0, ws.Position);
      Assert.AreEqual(0, rs.Position);

      ws.Write(rs);
      Assert.AreEqual(readData.Length, ms.Position);
      Assert.AreEqual(readData.Length, ws.Position);
      Assert.AreEqual(readData.Length, rs.Position);

      CollectionAssert.AreEqual(readData, ms.ToArray());
    }

  [Test]
  public void TestWriteRangedReadableStream() {
      var ms = new MemoryStream();
      var ws = new WritableStream(ms);

      var rs = new ReadableStream(new byte[] {5, 6, 7, 8, 9});

      var rrs = new RangedReadableSubstream(rs, 1, 3);
      rrs.Position = 1;

      Assert.AreEqual(0, ms.Position);
      Assert.AreEqual(0, ws.Position);

      ws.Write(rrs);
      Assert.AreEqual(3, ms.Position);
      Assert.AreEqual(3, ws.Position);

      CollectionAssert.AreEqual(new[] {6, 7, 8}, ms.ToArray());
    }
}