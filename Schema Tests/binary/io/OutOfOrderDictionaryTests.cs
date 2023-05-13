using System.Threading.Tasks;
using NUnit.Framework;


namespace schema.binary.io {
  public class OutOfOrderDictionaryTests {
    [Test]
    public async Task TestSetValueThenGet() {
      var impl = new OutOfOrderDictionary<string, string>();
      impl.Set("foo", "bar");
      Assert.AreEqual("bar", await impl.Get("foo"));
    }

    [Test]
    public async Task TestSetTaskThenGet() {
      var impl = new OutOfOrderDictionary<string, string>();
      impl.Set("foo", Task.FromResult("bar"));
      Assert.AreEqual("bar", await impl.Get("foo"));
    }

    [Test]
    public async Task TestGetThenSetValue() {
      var impl = new OutOfOrderDictionary<string, string>();

      var getTask = impl.Get("foo");
      impl.Set("foo", "bar");

      Assert.AreEqual("bar", await getTask);
    }

    [Test]
    public async Task TestGetThenSetTask() {
      var impl = new OutOfOrderDictionary<string, string>();

      var getTask = impl.Get("foo");
      impl.Set("foo", Task.FromResult("bar"));

      Assert.AreEqual("bar", await getTask);
    }
  }
}