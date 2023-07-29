using NUnit.Framework;


namespace schema.binary.io {
  public class UpDownStackTests {
    [Test]
    public void TestEmptyStack() {
      var impl = new UpDownStack<string>();

      Assert.AreEqual(0, impl.Count);
      Assert.AreEqual(false, impl.TryPeek(out _));
    }

    [Test]
    public void TestPushDown() {
      var impl = new UpDownStack<string>();
      impl.PushDownTo("foo");
      impl.PushDownTo("bar");

      Assert.AreEqual(2, impl.Count);
      Assert.AreEqual(true, impl.TryPeek(out var last));
      Assert.AreEqual("bar", last);
    }

    [Test]
    public void TestPushUp() {
      var impl = new UpDownStack<string>();
      impl.PushUpFrom("foo");
      impl.PushUpFrom("bar");

      Assert.AreEqual(2, impl.Count);
      Assert.AreEqual(true, impl.TryPeek(out var last));
      Assert.AreEqual("bar", last);
    }

    [Test]
    public void TestDownAndBackUp() {
      var impl = new UpDownStack<string>();
      impl.PushDownTo("foo");
      impl.PushDownTo("bar");
      impl.PushUpFrom("bar");
      impl.PushDownTo("bar");
      impl.PushUpFrom("bar");
      impl.PushUpFrom("foo");

      Assert.AreEqual(0, impl.Count);
      Assert.AreEqual(false, impl.TryPeek(out _));
    }

    [Test]
    public void TestUpAndBackDown() {
      var impl = new UpDownStack<string>();
      impl.PushUpFrom("foo");
      impl.PushUpFrom("bar");
      impl.PushDownTo("bar");
      impl.PushUpFrom("bar");
      impl.PushDownTo("bar");
      impl.PushDownTo("foo");

      Assert.AreEqual(0, impl.Count);
      Assert.AreEqual(false, impl.TryPeek(out _));
    }
  }
}