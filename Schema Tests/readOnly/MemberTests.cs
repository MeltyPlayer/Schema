using NUnit.Framework;

using schema.binary;

namespace schema.readOnly {
  internal class MemberTests {
    [Test]
    [TestCase("bool")]
    [TestCase("byte")]
    [TestCase("sbyte")]
    [TestCase("short")]
    [TestCase("ushort")]
    [TestCase("int")]
    [TestCase("uint")]
    [TestCase("long")]
    [TestCase("ulong")]
    [TestCase("float")]
    [TestCase("double")]
    [TestCase("char")]
    public void TestPrimitive(string primitiveType) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial interface IWrapper {
                public {{primitiveType}} Value { get; set; }
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public partial interface IWrapper : IReadOnlyWrapper {
                {{primitiveType}} IReadOnlyWrapper.Value => Value;
              }
              
              public interface IReadOnlyWrapper {
                public {{primitiveType}} Value { get; }
              }
            }

            """);
    }

    [Test]
    public void TestGeneric() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              public IEnumerable<bool> Value { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              System.Collections.Generic.IEnumerable<bool> IReadOnlyWrapper.Value => Value;
            }
            
            public interface IReadOnlyWrapper {
              public System.Collections.Generic.IEnumerable<bool> Value { get; }
            }
          }

          """);
    }

    [Test]
    public void TestNestedGeneric() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              public IEnumerable<IEnumerable<bool>> Value { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<bool>> IReadOnlyWrapper.Value => Value;
            }
            
            public interface IReadOnlyWrapper {
              public System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<bool>> Value { get; }
            }
          }

          """);
    }

    [Test]
    public void TestIndexer() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              public bool this[int index] { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              bool IReadOnlyWrapper.this[int index] => this[index];
            }
            
            public interface IReadOnlyWrapper {
              public bool this[int index] { get; }
            }
          }

          """);
    }

    [Test]
    public void TestNamelessTuple() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              public (bool, int) Tuple { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              (bool, int) IReadOnlyWrapper.Tuple => Tuple;
            }
            
            public interface IReadOnlyWrapper {
              public (bool, int) Tuple { get; }
            }
          }

          """);
    }

    [Test]
    public void TestNamedTuple() {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          """
          using schema.readOnly;
          using System.Collections.Generic;

          namespace foo.bar {
            [GenerateReadOnly]
            public partial interface IWrapper {
              public (bool a, int b) Tuple { get; set; }
            }
          }
          """,
          """
          namespace foo.bar {
            public partial interface IWrapper : IReadOnlyWrapper {
              (bool a, int b) IReadOnlyWrapper.Tuple => Tuple;
            }
            
            public interface IReadOnlyWrapper {
              public (bool a, int b) Tuple { get; }
            }
          }

          """);
    }
  }
}