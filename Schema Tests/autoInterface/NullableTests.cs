using NUnit.Framework;

using schema.binary;


namespace schema.autoInterface;

internal class NullableTests {
  [Test]
  public void TestSupportsNullablePrimitiveProperties() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper {
            public int? NullablePrimitive { get; set; }
          }
        }

        """,
        """
        namespace foo.bar {
          public partial class Wrapper : IWrapper;
          
          #nullable enable
          public interface IWrapper {
            public int? NullablePrimitive { get; set; }
          }
        }

        """);
  }

  [Test]
  public void TestSupportsNullableGenericProperties() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper<T> {
            public T? NullableGeneric { get; set; }
          }
        }

        """,
        """
        namespace foo.bar {
          public partial class Wrapper<T> : IWrapper<T>;
          
          #nullable enable
          public interface IWrapper<T> {
            public T? NullableGeneric { get; set; }
          }
        }

        """);
  }

  [Test]
  public void TestSupportsNullableGenericParameters() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;

        namespace foo.bar {
          [GenerateInterface]
          public partial class Wrapper<T> {
            public T? Method(T? t);
          }
        }

        """,
        """
        namespace foo.bar {
          public partial class Wrapper<T> : IWrapper<T>;
          
          #nullable enable
          public interface IWrapper<T> {
            public T? Method(T? t);
          }
        }

        """);
  }
}