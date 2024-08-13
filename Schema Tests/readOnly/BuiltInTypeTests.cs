using NUnit.Framework;

using schema.binary;


namespace schema.readOnly;

internal class BuiltInTypeTests {
  [Test]
  [TestCase("System.Span<int>",
            "System.ReadOnlySpan<int>",
            false)]
  [TestCase("System.Collections.Generic.ICollection<int>",
            "System.Collections.Generic.IReadOnlyCollection<int>",
            true)]
  [TestCase("System.Collections.Generic.IList<int>",
            "System.Collections.Generic.IReadOnlyList<int>",
            true)]
  public void TestSupportsEachBuiltInType(string mutable,
                                          string readOnly,
                                          bool needsToCast) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial interface IWrapper {
                {{mutable}} Property { get; set; }
            
                [Const]
                {{mutable}} Convert({{mutable}} value);
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public partial interface IWrapper : IReadOnlyWrapper {
                {{readOnly}} IReadOnlyWrapper.Property => {{(needsToCast ? $"({readOnly})(object) " : "")}}Property;
                {{readOnly}} IReadOnlyWrapper.Convert({{mutable}} value) => {{(needsToCast ? $"({readOnly})(object) " : "")}}Convert(value);
              }
              
              #nullable enable
              public interface IReadOnlyWrapper {
                public {{readOnly}} Property { get; }
                public {{readOnly}} Convert({{mutable}} value);
              }
            }

            """);
    }

  [Test]
  [TestCase("System.Span<int>")]
  [TestCase("System.Collections.Generic.ICollection<int>")]
  [TestCase("System.Collections.Generic.IList<int>")]
  public void TestDoesNotConvertBuiltInsForMutableProperties(string mutable) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial interface IWrapper {
                [KeepMutableType]
                {{mutable}} Property { get; set; }
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public partial interface IWrapper : IReadOnlyWrapper {
                {{mutable}} IReadOnlyWrapper.Property => Property;
              }
              
              #nullable enable
              public interface IReadOnlyWrapper {
                public {{mutable}} Property { get; }
              }
            }

            """);
    }

  [Test]
  [TestCase("System.Span<int>")]
  [TestCase("System.Collections.Generic.ICollection<int>")]
  [TestCase("System.Collections.Generic.IList<int>")]
  public void TestDoesNotConvertBuiltInsForMutableMethods(string mutable) {
      ReadOnlyGeneratorTestUtil.AssertGenerated(
          $$"""
            using schema.readOnly;

            namespace foo.bar {
              [GenerateReadOnly]
              public partial interface IWrapper {
                [Const]
                [KeepMutableType]
                {{mutable}} Convert({{mutable}} value);
              }
            }
            """,
          $$"""
            namespace foo.bar {
              public partial interface IWrapper : IReadOnlyWrapper {
                {{mutable}} IReadOnlyWrapper.Convert({{mutable}} value) => Convert(value);
              }
              
              #nullable enable
              public interface IReadOnlyWrapper {
                public {{mutable}} Convert({{mutable}} value);
              }
            }

            """);
    }
}