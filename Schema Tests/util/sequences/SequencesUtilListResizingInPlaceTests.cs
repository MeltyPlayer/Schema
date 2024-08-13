using System.Collections.Generic;

using NUnit.Framework;


namespace schema.util.sequences;

public class SequencesUtilListResizingInPlaceTests {
  [Test]
  public void TestResizeListNegativeThrowsError() {
      Assert.That(
          () => SequencesUtil.ResizeSequenceInPlace(
              new List<int> {0, 0, 0},
              -1),
          Throws.Exception);
    }

  [Test]
  public void TestResizeListGrowing() {
      var inputList = new List<int> {1, 2, 3};
      SequencesUtil.ResizeSequenceInPlace(inputList, 4);
      CollectionAssert.AreEqual(new List<int> {1, 2, 3, 0}, inputList);
    }

  [Test]
  public void TestResizeListShrinking() {
      var inputList = new List<int> {1, 2, 3};
      SequencesUtil.ResizeSequenceInPlace(inputList, 2);
      CollectionAssert.AreEqual(new List<int> {1, 2}, inputList);
    }

  [Test]
  public void TestResizingListLengthIsSame() {
      var inputList = new List<int> {1, 2, 3};
      SequencesUtil.ResizeSequenceInPlace(inputList, 3);
      CollectionAssert.AreEqual(new List<int> {1, 2, 3}, inputList);
    }
}