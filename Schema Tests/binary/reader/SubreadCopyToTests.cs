using System.IO;
using System.Linq;

using NUnit.Framework;

using schema.util.streams;

namespace schema.binary;

internal class SubreadCopyToTests {
  [Test]
  public void TestSubreadWithLengthCopyTo() {
    var ms = new MemoryStream(Enumerable.Range(0, 100)
                                        .Select(i => (byte) i)
                                        .ToArray());
    var outputMs = new MemoryStream();
    using var br = new SchemaBinaryReader(ms);
    br.Position = 25;
    br.Subread(50, () => br.CopyTo(outputMs));

    Assert.AreEqual(75, br.Position);
    Assert.AreEqual(50, outputMs.Position);
    CollectionAssert.AreEqual(
        Enumerable.Range(25, 50).Select(i => (byte) i),
        outputMs.GetBuffer().Take(50));
  }

  [Test]
  public void TestSubreadAtCopyTo() {
    var ms = new MemoryStream(Enumerable.Range(0, 100)
                                        .Select(i => (byte) i)
                                        .ToArray());
    var outputMs = new MemoryStream();
    using var br = new SchemaBinaryReader(ms);
    br.SubreadAt(
        25,
        () => br.CopyTo(outputMs));

    Assert.AreEqual(75, outputMs.Position);
    CollectionAssert.AreEqual(
        Enumerable.Range(25, 75).Select(i => (byte) i),
        outputMs.GetBuffer().Take(75));
  }

  [Test]
  public void TestSubreadAtWithLengthCopyTo() {
    var ms = new MemoryStream(Enumerable.Range(0, 100)
                                        .Select(i => (byte) i)
                                        .ToArray());
    var outputMs = new MemoryStream();
    using var br = new SchemaBinaryReader(ms);
    br.SubreadAt(
        25,
        50,
        () => br.CopyTo(outputMs));

    Assert.AreEqual(50, outputMs.Position);
    CollectionAssert.AreEqual(
        Enumerable.Range(25, 50).Select(i => (byte) i),
        outputMs.GetBuffer().Take(50));
  }
}