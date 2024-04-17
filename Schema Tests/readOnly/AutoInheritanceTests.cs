﻿using NUnit.Framework;

using schema.binary;

namespace fin.data.indexable {
  public interface IIndexable {
    int Index { get; }
  }
}


namespace schema.readOnly {
  internal class AutoInheritanceTests {
    [Test]
    public void TestWeirdSituation() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using fin.data.indexable;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial class AlreadyConstWrapper : IIndexable;
          }
          """,
          """
          namespace foo.bar {
            public partial class AlreadyConstWrapper : IReadOnlyAlreadyConstWrapper;
            
            public interface IReadOnlyAlreadyConstWrapper : fin.data.indexable.IIndexable;
          }

          """);
    }

    [Test]
    public void TestAlreadyConstInterfaceParent() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            public interface IAlreadyConst {
              public bool Property { get; }
            }
          
            [GenerateReadOnly]
            public partial class AlreadyConstWrapper : IAlreadyConst;
          }
          """,
          """
          namespace foo.bar {
            public partial class AlreadyConstWrapper : IReadOnlyAlreadyConstWrapper;
            
            public interface IReadOnlyAlreadyConstWrapper : IAlreadyConst;
          }

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
              public partial class AlreadyConstWrapper : place1.OtherParent.IAlreadyConst;
            }
          }
          """,
          """
          namespace foo.bar.place2 {
            public partial class Parent {
              public partial class AlreadyConstWrapper : IReadOnlyAlreadyConstWrapper;
              
              public interface IReadOnlyAlreadyConstWrapper : place1.OtherParent.IAlreadyConst;
            }
          }

          """);
    }

    [Test]
    public void TestAutoInheritance() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IBase {}
          
            [GenerateReadOnly]
            public partial interface IChild : IBase {}
          }
          """,
          """
          namespace foo.bar {
            public partial interface IBase : IReadOnlyBase;
            
            public interface IReadOnlyBase;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IChild : IReadOnlyChild;
            
            public interface IReadOnlyChild : IReadOnlyBase;
          }

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
          namespace foo.bar.other {
            public partial class OtherParent {
              public partial interface IBase : IReadOnlyBase;
              
              public interface IReadOnlyBase {
                public bool Foo { get; }
              }
            }
          }

          """,
          """
          namespace foo.bar {
            public partial class Parent {
              public partial interface IChild : IReadOnlyChild;
              
              public interface IReadOnlyChild : other.OtherParent.IReadOnlyBase {
                public bool Value { get; }
              }
            }
          }

          """);
    }

    [Test]
    public void TestAutoGenericInheritance() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IBase1<T> {}
          
            [GenerateReadOnly]
            public partial interface IBase2<T> {}
          
            [GenerateReadOnly]
            public partial interface IChild<T1, T2> : IBase1<T1>, IBase2<T2> {}
          }
          """,
          """
          namespace foo.bar {
            public partial interface IBase1<T> : IReadOnlyBase1<T>;
            
            public interface IReadOnlyBase1<T>;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IBase2<T> : IReadOnlyBase2<T>;
            
            public interface IReadOnlyBase2<T>;
          }

          """,
          """
          namespace foo.bar {
            public partial interface IChild<T1, T2> : IReadOnlyChild<T1, T2>;
            
            public interface IReadOnlyChild<T1, T2> : IReadOnlyBase1<T1>, IReadOnlyBase2<T2>;
          }

          """);
    }
  }
}