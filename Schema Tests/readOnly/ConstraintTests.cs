using NUnit.Framework;

using schema.binary;


namespace schema.readOnly {
  internal class ConstraintTests {
    [Test]
    [TestCase("class")]
    [TestCase("class?")]
    [TestCase("notnull")]
    [TestCase("struct")]
    [TestCase("unmanaged")]
    [TestCase("System.IO.Stream")]
    [TestCase("System.Collections.Generic.IEnumerable<T>")]
    public void TestEachConstraintType(string constraint) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial class EachConstraint<T> where T : {{constraint}} {
                [Const]
                public T Foo<S>(T t, S s) where S : {{constraint}} { }
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public partial class EachConstraint<T> : IReadOnlyEachConstraint<T> {
                T IReadOnlyEachConstraint<T>.Foo<S>(T t, S s) => Foo<S>(t, s);
              }
              
              public interface IReadOnlyEachConstraint<T> where T : {{constraint}} {
                public T Foo<S>(T t, S s) where S : {{constraint}};
              }
            }

            """);
    }

    [Test]
    public void TestCircularConstraints() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          
          namespace build.readOnly {
            [GenerateReadOnly]
            public partial interface IFinMatrix<[KeepMutableType] TMutable, TReadOnly, TImpl>
                where TMutable : IFinMatrix<TMutable, TReadOnly, TImpl>, TReadOnly
                where TReadOnly : IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> {
              [Const]
              TMutable CloneAndAdd(TReadOnly other);
          
              [Const]
              TMutable CloneAndAdd(in TImpl other);
            }
          }
          """,
          """
          namespace build.readOnly {
            public partial interface IFinMatrix<TMutable, TReadOnly, TImpl> : IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> {
              TMutable IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl>.CloneAndAdd(TReadOnly other) => CloneAndAdd(other);
              TMutable IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl>.CloneAndAdd(in TImpl other) => CloneAndAdd(in other);
            }
            
            public interface IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> where TMutable : IFinMatrix<TMutable, TReadOnly, TImpl>, TReadOnly where TReadOnly : build.readOnly.IReadOnlyFinMatrix<TMutable, TReadOnly, TImpl> {
              public TMutable CloneAndAdd(TReadOnly other);
              public TMutable CloneAndAdd(in TImpl other);
            }
          }
          
          """);
    }

    [Test]
    public void TestGenericSubConstraint() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial class SubConstraint<T1, T2> where T2 : T1 {
              [Const]
              public T1 Foo<S>(S s) where S : T1 { }
          
              public T2 Bar { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class SubConstraint<T1, T2> : IReadOnlySubConstraint<T1, T2> {
              T1 IReadOnlySubConstraint<T1, T2>.Foo<S>(S s) => Foo<S>(s);
              T2 IReadOnlySubConstraint<T1, T2>.Bar => Bar;
            }
            
            public interface IReadOnlySubConstraint<T1, T2> where T2 : T1 {
              public T1 Foo<S>(S s) where S : T1;
              public T2 Bar { get; }
            }
          }

          """);
    }

    [Test]
    public void TestMultipleGenericConstraints() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial class SimpleAttributes<T1, T2> where T1 : notnull, struct where T2 : unmanaged {
              [Const]
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) where T3 : class where T4 : class? { }
              
              public T2 Bar { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class SimpleAttributes<T1, T2> : IReadOnlySimpleAttributes<T1, T2> {
              T1 IReadOnlySimpleAttributes<T1, T2>.Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) => Foo<T3, T4>(t1, t2, t3, t4);
              T2 IReadOnlySimpleAttributes<T1, T2>.Bar => Bar;
            }
            
            public interface IReadOnlySimpleAttributes<T1, T2> where T1 : notnull, struct where T2 : unmanaged {
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) where T3 : class where T4 : class?;
              public T2 Bar { get; }
            }
          }

          """);
    }
  }
}