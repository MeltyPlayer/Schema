using NUnit.Framework;


namespace schema.util.enumerables;

public class EnumerableExtensionsTests {
  [Test]
  public void TestResizedOriginallyNull() {
      CollectionAssert.AreEqual(
          new[] {0, 0, 0},
          ((int[]?) null).Resized(3));
    }

  [Test]
  public void TestResizeArrayGrowing() {
      var inputList = new[] {1, 2, 3};
      CollectionAssert.AreEqual(new[] {1, 2, 3, 0}, inputList.Resized(4));
    }

  [Test]
  public void TestResizeArrayShrinking() {
      var inputList = new[] {1, 2, 3};
      CollectionAssert.AreEqual(new[] {1, 2}, inputList.Resized(2));
    }
}