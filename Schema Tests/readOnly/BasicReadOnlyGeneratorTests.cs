using NUnit.Framework;

using schema.binary;


namespace schema.readOnly;

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
          
          #nullable enable
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
            
            #nullable enable
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
          
          #nullable enable
          public interface IReadOnlySimpleGenerics<T1, in T2> {
            public T1 Foo<T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
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
          
          #nullable enable
          public interface IReadOnlyvoid<out @double> where @double : struct {
            public @void @int<@short>(@void @bool) where @short : @void;
            public @void @float { get; }
          }
        }

        """);
  }
}