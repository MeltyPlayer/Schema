using NUnit.Framework;

using schema.binary;


namespace schema.readOnly {
  internal class BasicReadOnlyGeneratorTests {
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
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial class Empty {
                {{emptySrc}}
              }
            }
            """,
          """
          namespace foo.bar {
            public partial class Empty : IReadOnlyEmpty;
            
            public interface IReadOnlyEmpty;
          }

          """);
    }

    [Test]
    // Special prefixes
    [TestCase("abstract ", "class", "BContainer", "IReadOnlyContainer")]
    [TestCase("", "interface", "IContainer", "IReadOnlyContainer")]
    // Prefix cases missed
    [TestCase("abstract ", "class", "Container", "IReadOnlyContainer")]
    [TestCase("", "interface", "Container", "IReadOnlyContainer")]
    [TestCase("", "class", "Bar", "IReadOnlyBar")]
    [TestCase("", "class", "Int", "IReadOnlyInt")]
    // Each other type
    [TestCase("", "class", "Container", "IReadOnlyContainer")]
    [TestCase("", "record", "Container", "IReadOnlyContainer")]
    [TestCase("", "record struct", "Container", "IReadOnlyContainer")]
    [TestCase("", "struct", "Container", "IReadOnlyContainer")]
    public void TestContainers(string containerPrefix,
                               string containerSuffix,
                               string containerName,
                               string readOnlyName) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public {{containerPrefix}}partial {{containerSuffix}} {{containerName}} {
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public {{containerPrefix}}partial {{containerSuffix}} {{containerName}} : {{readOnlyName}};
              
              public interface {{readOnlyName}};
            }

            """);
    }

    [Test]
    public void TestSimpleGenerics() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial class SimpleGenerics<T1, T2> {
              [Const]
              public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) { }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class SimpleGenerics<T1, T2> : IReadOnlySimpleGenerics<T1, T2> {
              T1 IReadOnlySimpleGenerics<T1, T2>.Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) => Foo<T3, T4>(t1, t2, t3, t4);
            }
            
            public interface IReadOnlySimpleGenerics<T1, T2> {
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

    [Test]
    public void TestKeywords() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace @const {
            [GenerateReadOnly]
            public partial class @void<@double> where @double : struct {
              [Const]
              public @void @int<@short>(@void @bool) where @short : @void { }
              
              public @void @float { get; }
            }
          }
          """,
          """
          namespace @const {
            public partial class @void<@double> : IReadOnlyvoid<@double> {
              @void IReadOnlyvoid<@double>.@int<@short>(@void @bool) => @int<@short>(@bool);
              @void IReadOnlyvoid<@double>.@float => @float;
            }
            
            public interface IReadOnlyvoid<@double> where @double : struct {
              public @void @int<@short>(@void @bool) where @short : @void;
              public @void @float { get; }
            }
          }

          """);
    }
  }
}