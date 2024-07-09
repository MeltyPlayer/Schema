using NUnit.Framework;

using schema.binary;

namespace schema.readOnly {
  internal class DefaultValueTests {
    [Test]
    [TestCase("null")]
    [TestCase("false")]
    [TestCase("true")]
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
              
              #nullable enable
              public interface IReadOnlyWrapper {
                public void Foo(bool? value = {{boolValue}});
              }
            }

            """);
    }

    [Test]
    [TestCase("null")]
    [TestCase("0")]
    [TestCase("-123")]
    [TestCase("123")]
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
              
              #nullable enable
              public interface IReadOnlyWrapper {
                public void Foo(int? value = {{intValue}});
              }
            }

            """);
    }

    [Test]
    [TestCase("null", "null")]
    [TestCase("SomeType.FOO", "(other.SomeType) 123")]
    public void
        TestSupportsDefaultEnums(string enumValue, string readonlyValue) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;
            using foo.bar.other;

            namespace foo.bar.other {
              public enum SomeType {
                FOO = 123,
              }
            }

            namespace foo.bar {
              [GenerateReadOnly]
              public partial interface IWrapper {
                [Const]
                public void Foo(SomeType? value = {{enumValue}});
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public partial interface IWrapper : IReadOnlyWrapper {
                void IReadOnlyWrapper.Foo(other.SomeType? value) => Foo(value);
              }
              
              #nullable enable
              public interface IReadOnlyWrapper {
                public void Foo(other.SomeType? value = {{readonlyValue}});
              }
            }

            """);
    }
  }
}