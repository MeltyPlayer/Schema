using NUnit.Framework;


namespace schema.readOnly;

internal class NullableTests {
  [Test]
  public void TestSupportsNullablePrimitiveProperties() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        
        namespace foo.bar;

        [GenerateReadOnly]
        public partial interface IWrapper {
          int? NullablePrimitive { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper : IReadOnlyWrapper {
          int? IReadOnlyWrapper.NullablePrimitive => NullablePrimitive;
        }
        
        public partial interface IReadOnlyWrapper {
          public int? NullablePrimitive { get; }
        }

        """);
  }

  [Test]
  public void TestSupportsNullableGenericProperties() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IWrapper<T> {
          T? NullableGeneric { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
          T? IReadOnlyWrapper<T>.NullableGeneric => NullableGeneric;
        }
        
        public partial interface IReadOnlyWrapper<out T> {
          public T? NullableGeneric { get; }
        }

        """);
  }

  [Test]
  public void TestSupportsNullableGenericParameters() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;

        [GenerateReadOnly]
        public partial interface IWrapper<T> {
          [Const]
          T? Method(T? t);
        }

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
          T? IReadOnlyWrapper<T>.Method(T? t) => Method(t);
        }
        
        public partial interface IReadOnlyWrapper<T> {
          public T? Method(T? t);
        }

        """);
  }
}