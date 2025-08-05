using NUnit.Framework;


namespace schema.readOnly;

internal class AutoInheritanceTests {
  [Test]
  [TestCase("System.Collections.Generic.IEnumerable<T>")]
  public void TestKnown(string knownBase) {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        $"""
          using schema.readOnly;

          namespace foo.bar;

          [GenerateReadOnly]
          public partial interface IGenericWrapper<T> : {knownBase};
          """,
        $"""
          #nullable enable

          namespace foo.bar;

          public partial interface IGenericWrapper<T> : IReadOnlyGenericWrapper<T>;

          public partial interface IReadOnlyGenericWrapper<out T> : {knownBase};

          """);
  }

  [Test]
  public void TestAlreadyConstInterfaceWithConstraint() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;

        public interface IAlreadyConst {
          public bool Property { get; }
        }

        [GenerateReadOnly]
        public partial class AlreadyConstWrapper<T> : IAlreadyConst where T : notnull;

        """,
        """
        #nullable enable

        namespace foo.bar;

        public partial class AlreadyConstWrapper<T> : IReadOnlyAlreadyConstWrapper<T>;

        public partial interface IReadOnlyAlreadyConstWrapper<out T> : IAlreadyConst where T : notnull;

        """);
  }

  [Test]
  public void TestAlreadyConstInterfaceParent() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        public interface IAlreadyConst {
          public bool Property { get; }
        }
      
        [GenerateReadOnly]
        public partial class AlreadyConstWrapper : IAlreadyConst;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial class AlreadyConstWrapper : IReadOnlyAlreadyConstWrapper;
        
        public partial interface IReadOnlyAlreadyConstWrapper : IAlreadyConst;

        """);
  }

  [Test]
  public void TestAlreadyConstInterfaceParentFromAnotherNamespace() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar.place1 {
          public class OtherParent {
            public interface IAlreadyConst {
              bool Property { get; }
            }
          }
        }

        namespace foo.bar.place2 {
          public partial class Parent {
            [GenerateReadOnly]
            public partial class AlreadyConstWrapper : foo.bar.place1.OtherParent.IAlreadyConst;
          }
        }

        """,
        """
        #nullable enable

        namespace foo.bar.place2;

        public partial class Parent {
          public partial class AlreadyConstWrapper : IReadOnlyAlreadyConstWrapper;
          
          public partial interface IReadOnlyAlreadyConstWrapper : foo.bar.place1.OtherParent.IAlreadyConst;
        }

        """);
  }

  [Test]
  public void TestAutoInheritance() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IBase {}
      
        [GenerateReadOnly]
        public partial interface IChild : IBase {}

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IBase : IReadOnlyBase;
        
        public partial interface IReadOnlyBase;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IChild : IReadOnlyChild;
        
        public partial interface IReadOnlyChild : IReadOnlyBase;

        """);
  }

  [Test]
  public void TestAutoInheritanceFromAnotherNamespace() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar.other {
          public partial class OtherParent {
            [GenerateReadOnly]
            public partial interface IBase {
              bool Foo { get; set; }
            }
          }
        }

        namespace foo.bar {
          public partial class Parent {
            [GenerateReadOnly]
            public partial interface IChild : other.OtherParent.IBase {
              public bool Value { get; set; }
            }
          }
        }

        """,
        """
        #nullable enable

        namespace foo.bar.other;

        public partial class OtherParent {
          public partial interface IBase : IReadOnlyBase {
            bool IReadOnlyBase.Foo => Foo;
          }
          
          public partial interface IReadOnlyBase {
            public bool Foo { get; }
          }
        }

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial class Parent {
          public partial interface IChild : IReadOnlyChild {
            bool IReadOnlyChild.Value => Value;
          }
          
          public partial interface IReadOnlyChild : other.OtherParent.IReadOnlyBase {
            public bool Value { get; }
          }
        }

        """);
  }

  [Test]
  public void TestAutoGenericInheritance() {
    ReadOnlyGeneratorTestUtil.AssertGenerated(
        """
        using schema.readOnly;

        namespace foo.bar;
        
        [GenerateReadOnly]
        public partial interface IBase1<T> {}
      
        [GenerateReadOnly]
        public partial interface IBase2<T> {}
      
        [GenerateReadOnly]
        public partial interface IChild<T1, T2> : IBase1<T1>, IBase2<T2> {}

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IBase1<T> : IReadOnlyBase1<T>;
        
        public partial interface IReadOnlyBase1<out T>;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IBase2<T> : IReadOnlyBase2<T>;
        
        public partial interface IReadOnlyBase2<out T>;

        """,
        """
        #nullable enable

        namespace foo.bar;
        
        public partial interface IChild<T1, T2> : IReadOnlyChild<T1, T2>;
        
        public partial interface IReadOnlyChild<T1, T2> : IReadOnlyBase1<T1>, IReadOnlyBase2<T2>;

        """);
  }
}