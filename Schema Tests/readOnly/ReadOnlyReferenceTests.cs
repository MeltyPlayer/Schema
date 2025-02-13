using NUnit.Framework;


namespace schema.readOnly;

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
        #nullable enable

        namespace foo.bar.other;

        public partial interface IOther : IReadOnlyOther;

        public partial interface IReadOnlyOther;

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper : IReadOnlyWrapper {
          other.IReadOnlyOther IReadOnlyWrapper.Field => Field;
        }

        public partial interface IReadOnlyWrapper {
          public other.IReadOnlyOther Field { get; }
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
        #nullable enable

        namespace foo.bar.other;

        public partial interface IOther : IReadOnlyOther;

        public partial interface IReadOnlyOther;

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper : IReadOnlyWrapper {
          other.IReadOnlyOther? IReadOnlyWrapper.Field => Field;
        }

        public partial interface IReadOnlyWrapper {
          public other.IReadOnlyOther? Field { get; }
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
        #nullable enable

        namespace foo.bar.other;

        public partial interface IOther : IReadOnlyOther;

        public partial interface IReadOnlyOther;

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper : IReadOnlyWrapper {
          System.Collections.Generic.IEnumerable<other.IReadOnlyOther> IReadOnlyWrapper.Field => Field;
        }

        public partial interface IReadOnlyWrapper {
          public System.Collections.Generic.IEnumerable<other.IReadOnlyOther> Field { get; }
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
        #nullable enable

        namespace foo.bar.other;

        public partial interface IOther : IReadOnlyOther;

        public partial interface IReadOnlyOther;

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper : IReadOnlyWrapper {
          (other.IReadOnlyOther, int) IReadOnlyWrapper.Field => Field;
        }

        public partial interface IReadOnlyWrapper {
          public (other.IReadOnlyOther, int) Field { get; }
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
        #nullable enable

        namespace foo.bar.other;

        public partial interface IOther : IReadOnlyOther;

        public partial interface IReadOnlyOther;

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper<T> : IReadOnlyWrapper<T>;

        public partial interface IReadOnlyWrapper<out T> where T : other.IReadOnlyOther;

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
        #nullable enable

        namespace foo.bar.other;

        public partial interface IOther : IReadOnlyOther;

        public partial interface IReadOnlyOther;

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper : IReadOnlyWrapper {
          void IReadOnlyWrapper.Foo<T>() => Foo<T>();
        }

        public partial interface IReadOnlyWrapper {
          public void Foo<T>() where T : other.IReadOnlyOther;
        }

        """);
  }

  [Test]
  public void TestInOtherType() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using foo.bar.other;

        namespace foo.bar;

        public partial class Parent {
          [GenerateReadOnly]
          public partial interface IOther;
        }

        [GenerateReadOnly]
        public partial interface IWrapper {
          Parent.IReadOnlyOther Field { get; set; }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial class Parent {
          public partial interface IOther : IReadOnlyOther;
          
          public partial interface IReadOnlyOther;
        }

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper : IReadOnlyWrapper {
          Parent.IReadOnlyOther IReadOnlyWrapper.Field => Field;
        }

        public partial interface IReadOnlyWrapper {
          public Parent.IReadOnlyOther Field { get; }
        }

        """);
  }

  [Test]
  public void TestGenericInOtherNamespace() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using foo.bar.wrong;
        using foo.bar.correct;

        namespace foo.bar.wrong {
          [GenerateReadOnly]
          public partial interface IOther;
        }

        namespace foo.bar.correct {
          [GenerateReadOnly]
          public partial interface IOther<T> : wrong.IOther;
        }

        namespace foo.bar {
          [GenerateReadOnly]
          public partial interface IWrapper<T> {
            IReadOnlyOther<T> Field { get; set; }
          }
        }

        """,
        """
        #nullable enable
        
        namespace foo.bar.wrong;

        public partial interface IOther : IReadOnlyOther;

        public partial interface IReadOnlyOther;

        """,
        """
        #nullable enable

        namespace foo.bar.correct;

        public partial interface IOther<T> : IReadOnlyOther<T>;
        
        public partial interface IReadOnlyOther<out T> : foo.bar.wrong.IReadOnlyOther;

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper<T> : IReadOnlyWrapper<T> {
          correct.IReadOnlyOther<T> IReadOnlyWrapper<T>.Field => Field;
        }
        
        public partial interface IReadOnlyWrapper<T> {
          public correct.IReadOnlyOther<T> Field { get; }
        }

        """);
  }

  [Test]
  public void TestIgnoresFakeMatches() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;
        using foo.bar.correct;
        using foo.bar.wrong;

        namespace foo.bar.correct {
          public class Other;
        }

        namespace foo.bar.correct {
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
        #nullable enable

        namespace foo.bar.correct;

        public partial interface IOther : IReadOnlyOther;

        public partial interface IReadOnlyOther;

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial interface IWrapper : IReadOnlyWrapper {
          correct.IReadOnlyOther IReadOnlyWrapper.Field => Field;
        }

        public partial interface IReadOnlyWrapper {
          public correct.IReadOnlyOther Field { get; }
        }

        """);
  }
}