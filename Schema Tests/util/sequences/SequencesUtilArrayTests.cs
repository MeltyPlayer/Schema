using NUnit.Framework;


namespace schema.util.sequences;

public class SequencesUtilArrayTests {
  [Test]
  public void TestResizeArrayNegativeThrowsError() {
      Assert.That(
          () => SequencesUtil.CloneAndResizeSequence((int[]?) null, -1),
          Throws.Exception);
    }

  [Test]
  public void TestResizeArrayOriginallyNull() {
      CollectionAssert.AreEqual(
          new[] {0, 0, 0},
          SequencesUtil.CloneAndResizeSequence((int[]?) null, 3));
    }

  [Test]
  public void TestResizeArrayGrowing() {
      var inputList = new[] {1, 2, 3};
      CollectionAssert.AreEqual(
          new[] {1, 2, 3, 0},
          SequencesUtil.CloneAndResizeSequence(inputList, 4));
    }

  [Test]
  public void TestResizeArrayShrinking() {
      var inputList = new[] {1, 2, 3};
      CollectionAssert.AreEqual(
          new[] {1, 2,},
          SequencesUtil.CloneAndResizeSequence(inputList, 2));
    }

  [Test]
  public void TestResizingArrayReturnsSameWhenLengthIsSame() {
      var inputList = new[] {1, 2, 3};
      Assert.AreSame(inputList,
                     SequencesUtil.CloneAndResizeSequence(
                         inputList,
                         inputList.Length));
    }
}