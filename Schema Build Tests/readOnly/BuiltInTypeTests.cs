using System;
using System.Collections.Generic;

using NUnit.Framework;

using schema.readOnly;


namespace readOnly {
  public partial class BuiltInTypeTests {
    [GenerateReadOnly]
    public partial class SpanWrapper<T> {
      [Const]
      public Span<T> Convert(Span<T> value) => value;
    }

    [Test]
    public void TestSpan() {
      IReadOnlySpanWrapper<int> wrapper = new SpanWrapper<int>();

      Span<int> expectedSpan = stackalloc int[3];
      var actualSpan = wrapper.Convert(expectedSpan);

      Assert.AreEqual(expectedSpan.Length, actualSpan.Length);
      for (var i = 0; i < expectedSpan.Length; ++i) {
        Assert.AreEqual(expectedSpan[i], actualSpan[i]);
      }
    }

    [GenerateReadOnly]
    public partial class ListWrapper<T> {
      [Const]
      public IList<T> Convert(IList<T> value) => value;
    }

    [Test]
    public void TestList() {
      IReadOnlyListWrapper<int> wrapper = new ListWrapper<int>();

      List<int> expectedList = [1, 2, 3];
      var actualList = wrapper.Convert(expectedList);

      Assert.AreSame(expectedList, actualList);
    }
  }
}