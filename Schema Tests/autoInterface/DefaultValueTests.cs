using NUnit.Framework;

using schema.binary;


namespace schema.autoInterface;

internal class DefaultValueTests {
  [Test]
  [TestCase("null")]
  [TestCase("false")]
  [TestCase("true")]
  public void TestSupportsDefaultBools(string boolValue) {
    InterfaceGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.autoInterface;

          namespace foo.bar {
            [GenerateInterface]
            public partial class Wrapper {
              [Const]
              public void Foo(bool? value = {{boolValue}});
            }
          }
          """,
        $$"""
          namespace foo.bar {
            public partial class Wrapper : IWrapper;
            
            #nullable enable
            public interface IWrapper {
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
    InterfaceGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.autoInterface;

          namespace foo.bar {
            [GenerateInterface]
            public partial class Wrapper {
              [Const]
              public void Foo(int? value = {{intValue}});
            }
          }
          """,
        $$"""
          namespace foo.bar {
            public partial class Wrapper : IWrapper;
            
            #nullable enable
            public interface IWrapper {
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
    InterfaceGeneratorTestUtil.AssertGenerated(
        $$"""
          using schema.autoInterface;
          using foo.bar.other;

          namespace foo.bar.other {
            public enum SomeType {
              FOO = 123,
            }
          }

          namespace foo.bar {
            [GenerateInterface]
            public partial class Wrapper {
              [Const]
              public void Foo(SomeType? value = {{enumValue}});
            }
          }
          """,
        $$"""
          namespace foo.bar {
            public partial class Wrapper : IWrapper;
            
            #nullable enable
            public interface IWrapper {
              public void Foo(other.SomeType? value = {{readonlyValue}});
            }
          }

          """);
  }
}