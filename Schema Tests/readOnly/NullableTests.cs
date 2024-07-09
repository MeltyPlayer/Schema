using NUnit.Framework;

using schema.binary;

namespace schema.readOnly {
  internal class NullableTests {
    [Test]
    public void TestSupportsNullablePrimitiveProperties() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              int? NullablePrimitive { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              int? IReadOnlyWrapper.NullablePrimitive => NullablePrimitive;
            }
            
            public interface IReadOnlyWrapper {
              public int? NullablePrimitive { get; }
            }
          }

          """);
    }

    [Test]
    public void TestSupportsNullableGenericProperties() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper<T> {
              T? NullableGeneric { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
              T? IReadOnlyWrapper<T>.NullableGeneric => NullableGeneric;
            }
            
            public interface IReadOnlyWrapper<out T> {
              public T? NullableGeneric { get; }
            }
          }

          """);
    }
  }
}