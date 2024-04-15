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
            public partial class Empty : IConstEmpty;
            
            public partial class IConstEmpty {
            }
          }

          """);
    }

    [Test]
    public void TestSimpleGenerics() {
      ConstGeneratorTestUtil.AssertGenerated(
          """
          using schema.@const;

          namespace foo.bar {
            [GenerateConst]
            public partial class SimpleGenerics<T1, T2> {
              [Const]
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) { }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class SimpleGenerics<T1, T2> : IConstSimpleGenerics<T1, T2>;
            
            public partial class IConstSimpleGenerics<T1, T2> {
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
            }
          }

          """);
    }

    [Test]
    [TestCase("class")]
    [TestCase("class?")]
    [TestCase("notnull")]
    [TestCase("struct")]
    [TestCase("unmanaged")]
    [TestCase("System.IO.Stream")]
    public void TestEachConstraintType(string constraint) {
      ConstGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.@const;

            namespace foo.bar {
              [GenerateConst]
              public partial class EachConstraint<T> where T : {{constraint}} {
                [Const]
                public T Foo<S>(T t, S s) where S : {{constraint}} { }
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public partial class EachConstraint<T> : IConstEachConstraint<T>;
              
              public partial class IConstEachConstraint<T> where T : {{constraint}} {
                public T Foo<S>(T t, S s) where S : {{constraint}};
              }
            }

            """);
    }

    [Test]
    public void TestGenericSubConstraint() {
      ConstGeneratorTestUtil.AssertGenerated(
          """
          using schema.@const;

          namespace foo.bar {
            [GenerateConst]
            public partial class SubConstraint<T1, T2> where T2 : T1 {
              [Const]
              public T1 Foo<S>(S s) where S : T1 { }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class SubConstraint<T1, T2> : IConstSubConstraint<T1, T2>;
            
            public partial class IConstSubConstraint<T1, T2> where T2 : T1 {
              public T1 Foo<S>(S s) where S : T1;
            }
          }

          """);
    }

    [Test]
    public void TestMultipleGenericConstraints() {
      ConstGeneratorTestUtil.AssertGenerated(
          """
          using schema.@const;

          namespace foo.bar {
            [GenerateConst]
            public partial class SimpleAttributes<T1, T2> where T1 : notnull, struct where T2 : unmanaged {
              [Const]
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) where T3 : class where T4 : class? { }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class SimpleAttributes<T1, T2> : IConstSimpleAttributes<T1, T2>;
            
            public partial class IConstSimpleAttributes<T1, T2> where T1 : notnull, struct where T2 : unmanaged {
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) where T3 : class where T4 : class?;
            }
          }

          """);
    }
  }
}