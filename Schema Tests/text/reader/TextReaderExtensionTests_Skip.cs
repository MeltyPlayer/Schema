using NUnit.Framework;


namespace schema.text.reader;

internal partial class TextReaderExtensionTests {
  [Test]
  [TestCase("", ExpectedResult = "")]
  [TestCase("    ", ExpectedResult = "")]
  [TestCase("    \n", ExpectedResult = "")]
  [TestCase("foo", ExpectedResult = "")]
  [TestCase("foo\nbar", ExpectedResult = "bar")]
  [TestCase("foo\r\nbar", ExpectedResult = "bar")]
  [TestCase("  foo  \n  bar  ", ExpectedResult = "  bar  ")]
  public string TestSkipToEndOfLine(string text) {
    using var tr = new SchemaTextReader(text);
    tr.SkipToEndOfLine();
    return tr.ReadRemainder();
  }
  
  [Test]
  [TestCase("", ExpectedResult = "")]
  [TestCase("    ", ExpectedResult = "")]
  [TestCase("  foo  ", ExpectedResult = "foo  ")]
  [TestCase(" \n\t foo", ExpectedResult = "foo")]
  [TestCase("  // 123\n foo", ExpectedResult = "foo")]
  [TestCase("  // 123\n// abc\n foo", ExpectedResult = "foo")]
  [TestCase("/* 123 \n \n *  */foo", ExpectedResult = "foo")]
  public string TestSkipCommentsAndWhitespace(string text) {
    using var tr = new SchemaTextReader(text);
    tr.SkipCommentsAndWhitespace();
    return tr.ReadRemainder();
  }
}