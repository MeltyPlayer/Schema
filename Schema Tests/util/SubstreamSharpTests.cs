﻿using System.IO;

using NUnit.Framework;

using schema.util.streams;

namespace schema.lib.SubstreamSharp {
  public class RangedSubstreamTests {
    [Test]
    public void TestFullSubstream() {
      var s = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7 });
      var ss = new RangedSubstream(s, 0, s.Length);

      Assert.AreEqual(0, s.Position);
      Assert.AreEqual(0, ss.Position);

      Assert.AreEqual(7, ss.Length);
      Assert.AreEqual(7, ss.Length);
    }
  }
}