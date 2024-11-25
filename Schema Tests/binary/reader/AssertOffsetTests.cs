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

    Assert.DoesNotThrow(() => br.SubreadAt(5, 5, (_) => { }));
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
        () => br.SubreadAt(8, 5, (_) => { }));
  }

  [Test]
  public void TestNestedSubreads_NotThrowing() {
    var data = new byte[100];
    var ms = new MemoryStream(data);
    using var br = new SchemaBinaryReader(ms) { AssertAlreadyAtOffset = true };

    br.Position = 5;
    br.SubreadAt(
        0,
        sbr => {
          sbr.Position = 10;
          Assert.DoesNotThrow(() => sbr.SubreadAt(10, 5, (_) => { }));
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
        sbr => {
          sbr.Position = 10;
          Assert.Throws<Asserts.AssertionException>(
              () => sbr.SubreadAt(8, 5, (_) => { }));
        });
  }
}