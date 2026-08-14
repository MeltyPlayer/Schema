using NUnit.Framework;


namespace schema.text.reader;

internal class SchemaTextReaderSkipTests {
  [Test]
  [TestCase("", ExpectedResult = "")]
  [TestCase("foo", ExpectedResult = "")]
  [TestCase("foo_bar", ExpectedResult = "_bar")]
  [TestCase("foo_bar_abc", ExpectedResult = "_bar_abc")]
  public string TestSkipUpToStartOfTerminator_Char(string text) {
    using var tr = new SchemaTextReader(text);
    tr.SkipUpToStartOfTerminator('_');
    return tr.ReadRemainder();
  }

  [Test]
  [TestCase("", ExpectedResult = "")]
  [TestCase("foo", ExpectedResult = "")]
  [TestCase("foo123bar", ExpectedResult = "123bar")]
  [TestCase("foo123bar123abc", ExpectedResult = "123bar123abc")]
  public string TestSkipUpToStartOfTerminator_String(string text) {
    using var tr = new SchemaTextReader(text);
    tr.SkipUpToStartOfTerminator("123");
    return tr.ReadRemainder();
  }

  [Test]
  [TestCase("", ExpectedResult = "")]
  [TestCase("foo", ExpectedResult = "")]
  [TestCase("foo_bar", ExpectedResult = "bar")]
  [TestCase("foo_bar_abc", ExpectedResult = "bar_abc")]
  public string TestSkipUpToAndPastTerminator_Char(string text) {
    using var tr = new SchemaTextReader(text);
    tr.SkipUpToAndPastTerminator('_');
    return tr.ReadRemainder();
  }

  [Test]
  [TestCase("", ExpectedResult = "")]
  [TestCase("foo", ExpectedResult = "")]
  [TestCase("foo123bar", ExpectedResult = "bar")]
  [TestCase("foo123bar123abc", ExpectedResult = "bar123abc")]
  public string TestSkipUpToAndPastTerminator_String(string text) {
    using var tr = new SchemaTextReader(text);
    tr.SkipUpToAndPastTerminator("123");
    return tr.ReadRemainder();
  }
}