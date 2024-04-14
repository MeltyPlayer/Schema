using NUnit.Framework;

using schema.binary;


namespace schema.@const {
  internal class BasicConstGeneratorTests {
    [Test]
    // Unsupported
    [TestCase("public bool field;")]
    [TestCase("public bool Field { set; }")]
    // Missing const attribute
    [TestCase("public bool Field { get; set; }")]
    [TestCase("public bool Field();")]
    // Not accessible enough
    [TestCase("[Const] private bool Field { get; set; }")]
    [TestCase("[Const] bool Field();")]
    public void TestEmpty(string emptySrc) {
      ConstGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.@const;

            namespace foo.bar {
              [GenerateConst]
              public partial class Empty {
                {{emptySrc}}
              }
            }
            """,
          """
          namespace foo.bar {
            public partial class IConstEmpty {
            }
            
            public partial class Empty : IConstEmpty {
            }
          }

          """);
    }

    [Test]
    public void TestSimpleAttributes() {
      ConstGeneratorTestUtil.AssertGenerated(
          """
          using schema.@const;

          namespace foo.bar {
            [GenerateConst]
            public partial class SimpleAttributes<T1, T2> {
              [Const]
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) { }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class IConstSimpleAttributes<T1, T2> {
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
            }
            
            public partial class SimpleAttributes<T1, T2> : IConstSimpleAttributes<T1, T2> {
            }
          }

          """);
    }
  }
}