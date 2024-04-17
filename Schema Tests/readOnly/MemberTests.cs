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
              public partial interface IWrapper : IReadOnlyWrapper;
              
              public interface IReadOnlyWrapper {
                public {{primitiveType}} Value { get; }
              }
            }

            """);
    }

    [Test]
    public void TestIEnumerable() {
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
            public partial interface IWrapper : IReadOnlyWrapper;
            
            public interface IReadOnlyWrapper {
              public System.Collections.Generic.IEnumerable<bool> Value { get; }
            }
          }

          """);
    }
  }
}