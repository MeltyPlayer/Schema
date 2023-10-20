using System;

using NUnit.Framework;

using schema.util.asserts;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace schema.util.streams {
  public class RangedReadableSubstreamTests {
    [Test]
    public void TestFullSubstream() {
      var data = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
      var rs = new ReadableStream(data);
      var rrs = new RangedReadableSubstream(rs, 0, rs.Length);

      Assert.AreEqual(0, rs.Position);
      Assert.AreEqual(0, rrs.Position);
      Assert.AreEqual(7, rs.Length);
      Assert.AreEqual(7, rrs.Length);

      CollectionAssert.AreEqual(data, rrs.ReadAllBytes());
      Assert.AreEqual(7, rs.Position);
      Assert.AreEqual(7, rrs.Position);
    }

    [Test]
    public void TestPartialSubstream() {
      var data = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
      var rs = new ReadableStream(data);

      var rrs = new RangedReadableSubstream(rs, 1, rs.Length - 2);
      rrs.Position = 1;

      Assert.AreEqual(1, rs.Position);
      Assert.AreEqual(1, rrs.Position);
      Assert.AreEqual(7, rs.Length);
      Assert.AreEqual(6, rrs.Length);

      CollectionAssert.AreEqual(data.AsSpan(1, data.Length - 2).ToArray(),
                                rrs.ReadAllBytes());
      Assert.AreEqual(6, rs.Position);
      Assert.AreEqual(6, rrs.Position);
    }
  }
}