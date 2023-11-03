using System.Collections.Generic;

using NUnit.Framework;

namespace schema.util.sequences {
  public class SequencesUtilReadOnlyListTests {
    [Test]
    public void TestResizeListNegativeThrowsError() {
      Assert.That(
          () => SequencesUtil.CloneAndResizeSequence(
              (IReadOnlyList<int>?) null,
              -1),
          Throws.Exception);
    }

    [Test]
    public void TestResizeListOriginallyNull() {
      CollectionAssert.AreEqual(
          new List<int> { 0, 0, 0 },
          SequencesUtil.CloneAndResizeSequence((IReadOnlyList<int>?) null, 3));
    }

    [Test]
    public void TestResizeListGrowing() {
      var inputList = new List<int> { 1, 2, 3 };

      var resizedList =
          SequencesUtil.CloneAndResizeSequence((IReadOnlyList<int>) inputList,
                                               4);

      Assert.AreNotSame(inputList, resizedList);
      CollectionAssert.AreEqual(new List<int> { 1, 2, 3, 0 }, resizedList);
    }

    [Test]
    public void TestResizeListShrinking() {
      var inputList = new List<int> { 1, 2, 3 };

      var resizedList =
          SequencesUtil.CloneAndResizeSequence((IReadOnlyList<int>) inputList,
                                               2);

      Assert.AreNotSame(inputList, resizedList);
      CollectionAssert.AreEqual(new List<int> { 1, 2 }, resizedList);
    }

    [Test]
    public void TestResizingListLengthIsSame() {
      var inputList = new List<int> { 1, 2, 3 };

      var resizedList =
          SequencesUtil.CloneAndResizeSequence((IReadOnlyList<int>) inputList,
                                               3);

      Assert.AreSame(inputList, resizedList);
      CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, resizedList);
    }
  }
}