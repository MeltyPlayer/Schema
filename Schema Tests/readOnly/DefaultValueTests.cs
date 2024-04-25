using NUnit.Framework;

using schema.binary;

namespace schema.readOnly {
  internal class DefaultValueTests {
    [Test]
    [TestCase("false")]
    [TestCase("true")]
    [TestCase("null")]
    public void TestSupportsDefaultBools(string boolValue) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial interface IWrapper {
                [Const]
                public void Foo(bool? value = {{boolValue}});
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public partial interface IWrapper : IReadOnlyWrapper {
                void IReadOnlyWrapper.Foo(bool? value) => Foo(value);
              }
              
              public interface IReadOnlyWrapper {
                public void Foo(bool? value = {{boolValue}});
              }
            }

            """);
    }

    [Test]
    [TestCase("0")]
    [TestCase("-123")]
    [TestCase("123")]
    [TestCase("null")]
    public void TestSupportsDefaultInts(string intValue) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial interface IWrapper {
                [Const]
                public void Foo(int? value = {{intValue}});
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public partial interface IWrapper : IReadOnlyWrapper {
                void IReadOnlyWrapper.Foo(int? value) => Foo(value);
              }
              
              public interface IReadOnlyWrapper {
                public void Foo(int? value = {{intValue}});
              }
            }

            """);
    }
  }
}