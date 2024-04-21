using NUnit.Framework;

using schema.binary;

namespace schema.readOnly {
  internal class ReadOnlyReferenceTests {
    [Test]
    public void TestInOtherNamespace() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using foo.bar.other;

          namespace foo.bar.other {
            [GenerateReadOnly]
            public partial interface IOther;
          }

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              IReadOnlyOther Field { get; set; }
            }
          }
          """,
          """
          namespace foo.bar.other {
            public partial interface IOther : IReadOnlyOther;
            
            public interface IReadOnlyOther;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              other.IReadOnlyOther IReadOnlyWrapper.Field => Field;
            }
            
            public interface IReadOnlyWrapper {
              public other.IReadOnlyOther Field { get; }
            }
          }

          """);
    }

    [Test]
    public void TestNullableInOtherNamespace() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using foo.bar.other;

          namespace foo.bar.other {
            [GenerateReadOnly]
            public partial interface IOther;
          }

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              IReadOnlyOther? Field { get; set; }
            }
          }
          """,
          """
          namespace foo.bar.other {
            public partial interface IOther : IReadOnlyOther;
            
            public interface IReadOnlyOther;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              other.IReadOnlyOther? IReadOnlyWrapper.Field => Field;
            }
            
            public interface IReadOnlyWrapper {
              public other.IReadOnlyOther? Field { get; }
            }
          }

          """);
    }

    [Test]
    public void TestIEnumerableInOtherNamespace() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using foo.bar.other;
          using System.Collections.Generic;
          
          namespace foo.bar.other {
            [GenerateReadOnly]
            public partial interface IOther;
          }

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              IEnumerable<IReadOnlyOther> Field { get; set; }
            }
          }
          """,
          """
          namespace foo.bar.other {
            public partial interface IOther : IReadOnlyOther;
            
            public interface IReadOnlyOther;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              System.Collections.Generic.IEnumerable<other.IReadOnlyOther> IReadOnlyWrapper.Field => Field;
            }
            
            public interface IReadOnlyWrapper {
              public System.Collections.Generic.IEnumerable<other.IReadOnlyOther> Field { get; }
            }
          }

          """);
    }

    [Test]
    public void TestTupleInOtherNamespace() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using foo.bar.other;

          namespace foo.bar.other {
            [GenerateReadOnly]
            public partial interface IOther;
          }

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              (IReadOnlyOther, int) Field { get; set; }
            }
          }
          """,
          """
          namespace foo.bar.other {
            public partial interface IOther : IReadOnlyOther;
            
            public interface IReadOnlyOther;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              (other.IReadOnlyOther, int) IReadOnlyWrapper.Field => Field;
            }
            
            public interface IReadOnlyWrapper {
              public (other.IReadOnlyOther, int) Field { get; }
            }
          }

          """);
    }

    [Test]
    public void TestTypeConstraintInOtherNamespace() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using foo.bar.other;

          namespace foo.bar.other {
            [GenerateReadOnly]
            public partial interface IOther;
          }

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper<T> where T : IReadOnlyOther;
          }
          """,
          """
          namespace foo.bar.other {
            public partial interface IOther : IReadOnlyOther;
            
            public interface IReadOnlyOther;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IWrapper<T> : IReadOnlyWrapper<T>;
            
            public interface IReadOnlyWrapper<T> where T : other.IReadOnlyOther;
          }

          """);
    }

    [Test]
    public void TestMethodConstraintInOtherNamespace() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using foo.bar.other;

          namespace foo.bar.other {
            [GenerateReadOnly]
            public partial interface IOther;
          }

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              [Const]
              public void Foo<T>() where T : IReadOnlyOther;
            }
          }
          """,
          """
          namespace foo.bar.other {
            public partial interface IOther : IReadOnlyOther;
            
            public interface IReadOnlyOther;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              void IReadOnlyWrapper.Foo<T>() => Foo<T>();
            }
            
            public interface IReadOnlyWrapper {
              public void Foo<T>() where T : other.IReadOnlyOther;
            }
          }

          """);
    }

    [Test]
    public void TestInOtherType() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using foo.bar.other;

          namespace foo.bar {
            public partial class Parent {
              [GenerateReadOnly]
              public partial interface IOther;
            }
          
            [GenerateReadOnly]
            public partial interface IWrapper {
              Parent.IReadOnlyOther Field { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial class Parent {
              public partial interface IOther : IReadOnlyOther;
              
              public interface IReadOnlyOther;
            }
          }

          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              Parent.IReadOnlyOther IReadOnlyWrapper.Field => Field;
            }
            
            public interface IReadOnlyWrapper {
              public Parent.IReadOnlyOther Field { get; }
            }
          }

          """);
    }
  }
}