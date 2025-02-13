using NUnit.Framework;


namespace schema.readOnly;

internal class SameNameTests {
  [Test]
  public void TestSameName() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface ISameName;
      
        [GenerateReadOnly]
        public partial interface ISameName<T> : ISameName;
        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface ISameName : IReadOnlySameName;
        
        public partial interface IReadOnlySameName;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface ISameName<T> : IReadOnlySameName<T>;
        
        public partial interface IReadOnlySameName<out T> : IReadOnlySameName;

        """);
  }
}