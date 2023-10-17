using NUnit.Framework;

namespace schema.util.streams {
  public class RangedReadableSubstreamTests {
    [Test]
    public void TestFullSubstream() {
      var s = new ReadableStream(new byte[] { 1, 2, 3, 4, 5, 6, 7 });
      var ss = new RangedReadableSubstream(s, 0, s.Length);

      Assert.AreEqual(0, s.Position);
      Assert.AreEqual(0, ss.Position);

      Assert.AreEqual(7, ss.Length);
      Assert.AreEqual(7, ss.Length);
    }
  }
}