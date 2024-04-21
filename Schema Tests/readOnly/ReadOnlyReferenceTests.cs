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