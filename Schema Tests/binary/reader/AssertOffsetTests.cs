using System;
using System.IO;

using NUnit.Framework;

using schema.util.asserts;


namespace schema.binary;

public class AssertOffsetTests {
  [Test]
  public void TestNestedSpaces_NotThrowing() {
    var data = new byte[100];
    var ms = new MemoryStream(data);
    using var br = new SchemaBinaryReader(ms) { AssertAlreadyAtOffset = true };

    br.Position = 5;

    br.PushLocalSpace();
    br.Position = 5;

    br.PushLocalSpace();
    br.Position = 5;

    Assert.DoesNotThrow(() => br.SubreadAt(5, 5, () => { }));
  }

  [Test]
  public void TestNestedSpaces_Throwing() {
    var data = new byte[100];
    var ms = new MemoryStream(data);
    using var br = new SchemaBinaryReader(ms) { AssertAlreadyAtOffset = true };

    br.Position = 5;

    br.PushLocalSpace();
    br.Position = 5;

    br.PushLocalSpace();
    br.Position = 5;

    Assert.Throws<Asserts.AssertionException>(
        () => br.SubreadAt(8, 5, () => { }));
  }

  [Test]
  public void TestNestedSubreads_NotThrowing() {
    var data = new byte[100];
    var ms = new MemoryStream(data);
    using var br = new SchemaBinaryReader(ms) { AssertAlreadyAtOffset = true };

    br.Position = 5;
    br.SubreadAt(
        0,
        () => {
          br.Position = 10;
          Assert.DoesNotThrow(() => br.SubreadAt(10, 5, () => { }));
        });
  }

  [Test]
  public void TestNestedSubreads_Throwing() {
    var data = new byte[100];
    var ms = new MemoryStream(data);
    using var br = new SchemaBinaryReader(ms) { AssertAlreadyAtOffset = true };

    br.Position = 5;
    br.SubreadAt(
        0,
        () => {
          br.Position = 10;
          Assert.Throws<Asserts.AssertionException>(
              () => br.SubreadAt(8, 5, () => { }));
        });
  }
}