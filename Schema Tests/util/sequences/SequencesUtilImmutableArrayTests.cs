using System.Collections.Immutable;

using NUnit.Framework;

namespace schema.util.sequences {
  public class SequencesUtilImmutableArrayTests {
    [Test]
    public void TestResizeArrayNegativeThrowsError() {
      Assert.That(
          () => SequencesUtil.CloneAndResizeSequence(
              (ImmutableArray<int>?) null,
              -1),
          Throws.Exception);
    }

    [Test]
    public void TestResizeArrayOriginallyNull() {
      CollectionAssert.AreEqual(
          new[] { 0, 0, 0 },
          SequencesUtil.CloneAndResizeSequence((ImmutableArray<int>?) null, 3));
    }

    [Test]
    public void TestResizeArrayGrowing() {
      var inputList =
          (ImmutableArray<int>?) new[] { 1, 2, 3 }.ToImmutableArray();
      CollectionAssert.AreEqual(
          new[] { 1, 2, 3, 0 },
          SequencesUtil.CloneAndResizeSequence(inputList, 4));
    }

    [Test]
    public void TestResizeArrayShrinking() {
      var inputList =
          (ImmutableArray<int>?) new[] { 1, 2, 3 }.ToImmutableArray();
      CollectionAssert.AreEqual(
          new[] { 1, 2, },
          SequencesUtil.CloneAndResizeSequence(inputList, 2));
    }

    [Test]
    public void TestResizingArrayReturnsSameWhenLengthIsSame() {
      var inputList =
          (ImmutableArray<int>?) new[] { 1, 2, 3 }.ToImmutableArray();
      CollectionAssert.AreEqual(
          new[] { 1, 2, 3 },
          SequencesUtil.CloneAndResizeSequence(inputList, 3));
    }
  }
}