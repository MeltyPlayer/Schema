using NUnit.Framework;


namespace schema.text.reader;

internal class TextReaderExtensionTests {
  [Test]
  // Empty cases
  [TestCase("", ExpectedResult = new string[] { })]
  [TestCase(" \n  \t ", ExpectedResult = new string[] { })]
  [TestCase(" ) ", ExpectedResult = new string[] { })]
  // One argument cases
  [TestCase("foo", ExpectedResult = new[] { "foo" })]
  [TestCase("  \n foo  \t ", ExpectedResult = new[] { "foo" })]
  [TestCase("foo )", ExpectedResult = new[] { "foo" })]
  // Multiple argument cases
  [TestCase("foo,bar,123,abc", ExpectedResult = new[] { "foo", "bar", "123", "abc" })]
  [TestCase("foo,bar,123,abc,", ExpectedResult = new[] { "foo", "bar", "123", "abc" })]
  [TestCase("  foo , bar , 123, abc  ", ExpectedResult = new[] { "foo", "bar", "123", "abc" })]
  // Mid-space cases
  [TestCase(" foo bar ", ExpectedResult = new[] { "foo bar" })]
  [TestCase("foo \t\n bar", ExpectedResult = new[] { "foo \t\n bar" })]
  // Quote cases
  [TestCase("  'foo'  ", ExpectedResult = new[] { "'foo'" })]
  [TestCase("'foo bar'", ExpectedResult = new[] { "'foo bar'" })]
  [TestCase("'foo, bar'", ExpectedResult = new[] { "'foo, bar'" })]
  [TestCase("'foo\"'", ExpectedResult = new[] { "'foo\"'" })]
  [TestCase("\"foo bar\"", ExpectedResult = new[] { "\"foo bar\"" })]
  [TestCase("`foo bar`", ExpectedResult = new[] { "`foo bar`" })]
  public string[] TestReadArguments(string text) {
    using var tr = TextSchemaTestUtil.CreateTextReader(text);
    return tr.ReadArguments([','], [')']);
  }
}