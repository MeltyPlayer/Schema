using NUnit.Framework;

using schema.binary;


namespace schema.@const {
  internal class BasicConstGeneratorTests {
    [Test]
    // Unsupported
    [TestCase("public bool field;")]
    [TestCase("public bool Field { set; }")]
    // Missing const attribute
    [TestCase("public bool Field();")]
    // Not accessible enough
    [TestCase("private bool Field { get; set; }")]
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
            
            public interface IConstEmpty {
            }
          }

          """);
    }

    [Test]
    [TestCase("abstract ", "class", "B")]
    [TestCase("", "class")]
    [TestCase("", "interface", "I")]
    [TestCase("", "record")]
    [TestCase("", "record struct")]
    [TestCase("", "struct")]
    public void TestContainers(string containerPrefix,
                               string containerSuffix,
                               string namePrefix = "") {
      ConstGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.@const;

            namespace foo.bar {
              [GenerateConst]
              public {{containerPrefix}}partial {{containerSuffix}} {{namePrefix}}Container {
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public {{containerPrefix}}partial {{containerSuffix}} {{namePrefix}}Container : IConstContainer;
              
              public interface IConstContainer {
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
            
            public interface IConstSimpleGenerics<T1, T2> {
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
              
              public interface IConstEachConstraint<T> where T : {{constraint}} {
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
          
              public T2 Bar { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class SubConstraint<T1, T2> : IConstSubConstraint<T1, T2>;
            
            public interface IConstSubConstraint<T1, T2> where T2 : T1 {
              public T1 Foo<S>(S s) where S : T1;
              public T2 Bar { get; }
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
              
              public T2 Bar { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class SimpleAttributes<T1, T2> : IConstSimpleAttributes<T1, T2>;
            
            public interface IConstSimpleAttributes<T1, T2> where T1 : notnull, struct where T2 : unmanaged {
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) where T3 : class where T4 : class?;
              public T2 Bar { get; }
            }
          }

          """);
    }

    [Test]
    public void TestKeywords() {
      ConstGeneratorTestUtil.AssertGenerated(
          """
          using schema.@const;

          namespace @const {
            [GenerateConst]
            public partial class @void<@double> where @double : struct {
              [Const]
              public @void @int<@short>(@void @bool) where @short : @void { }
              
              public @void @float { get; }
            }
          }
          """,
          """
          namespace @const {
            public partial class @void<@double> : IConstvoid<@double>;
            
            public interface IConstvoid<@double> where @double : struct {
              public @void @int<@short>(@void @bool) where @short : @void;
              public @void @float { get; }
            }
          }

          """);
    }
  }
}