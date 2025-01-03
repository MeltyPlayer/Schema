using NUnit.Framework;


namespace schema.autoInterface;

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
    InterfaceGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.autoInterface;

          namespace foo.bar;
          
          [GenerateInterface]
          public partial class EachConstraint<T> where T : {{constraint}} {
            public T Foo<S>(T t, S s) where S : {{constraint}} { }
          }
          """,
        $$"""
          namespace foo.bar;
          
          public partial class EachConstraint<T> : IEachConstraint<T>;
          
          #nullable enable
          public interface IEachConstraint<T> where T : {{constraint}} {
            public T Foo<S>(T t, S s) where S : {{constraint}};
          }

          """);
  }

  [Test]
  public void TestCircularConstraints() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;

        namespace foo.bar;
        
        [GenerateInterface]
        public partial class Circular<[KeepMutableType] TMutable, TInterface, TImpl>
            where TMutable : Circular<TMutable, TInterface, TImpl>, TInterface
            where TInterface : ICircular<TMutable, TInterface, TImpl> {
          public TMutable Foo(TInterface other);
      
          public TMutable Foo(in TImpl other);
        }
        """,
        """
        namespace foo.bar;
        
        public partial class Circular<TMutable, TInterface, TImpl> : ICircular<TMutable, TInterface, TImpl>;
        
        #nullable enable
        public interface ICircular<TMutable, TInterface, TImpl> where TMutable : Circular<TMutable, TInterface, TImpl>, TInterface where TInterface : ICircular<TMutable, TInterface, TImpl> {
          public TMutable Foo(TInterface other);
          public TMutable Foo(in TImpl other);
        }

        """);
  }

  [Test]
  public void TestGenericSubConstraint() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;

        namespace foo.bar;
        
        [GenerateInterface]
        public partial class SubConstraint<T1, T2> where T2 : T1 {
          public T1 Foo<S>(S s) where S : T1 { }
      
          public T2 Bar { get; set; }
        }
        """,
        """
        namespace foo.bar;
        
        public partial class SubConstraint<T1, T2> : ISubConstraint<T1, T2>;
        
        #nullable enable
        public interface ISubConstraint<T1, T2> where T2 : T1 {
          public T1 Foo<S>(S s) where S : T1;
          public T2 Bar { get; set; }
        }

        """);
  }

  [Test]
  public void TestMultipleGenericConstraints() {
    InterfaceGeneratorTestUtil.AssertGenerated(
        """
        using schema.autoInterface;

        namespace foo.bar;

        [GenerateInterface]
        public partial class SimpleAttributes<T1, T2> where T1 : notnull, struct where T2 : unmanaged {
          public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) where T3 : class where T4 : class? { }
          
          public T2 Bar { get; set; }
        }
        """,
        """
        namespace foo.bar;

        public partial class SimpleAttributes<T1, T2> : ISimpleAttributes<T1, T2>;
        
        #nullable enable
        public interface ISimpleAttributes<T1, T2> where T1 : notnull, struct where T2 : unmanaged {
          public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) where T3 : class where T4 : class?;
          public T2 Bar { get; set; }
        }

        """);
  }
}