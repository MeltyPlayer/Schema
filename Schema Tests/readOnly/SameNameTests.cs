using NUnit.Framework;

using schema.binary;


namespace schema.readOnly;

internal class SameNameTests {
  [Test]
  public void TestSameName() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar {
          [GenerateReadOnly]
          public partial interface ISameName;
        
          [GenerateReadOnly]
          public partial interface ISameName<T> : ISameName;
        }
        """,
        """
        namespace foo.bar {
          public partial interface ISameName : IReadOnlySameName;
          
          #nullable enable
          public partial interface IReadOnlySameName;
        }

        """,
        """
        namespace foo.bar {
          public partial interface ISameName<T> : IReadOnlySameName<T>;
          
          #nullable enable
          public partial interface IReadOnlySameName<out T> : IReadOnlySameName;
        }

        """);
  }
}