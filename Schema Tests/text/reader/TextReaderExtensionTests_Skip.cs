using NUnit.Framework;


namespace schema.text.reader;

internal partial class TextReaderExtensionTests {
  [Test]
  [TestCase("", ExpectedResult = "")]
  [TestCase("    ", ExpectedResult = "")]
  [TestCase("  foo  ", ExpectedResult = "foo  ")]
  [TestCase(" \n\t foo", ExpectedResult = "foo")]
  [TestCase("  // 123\n foo", ExpectedResult = "foo")]
  [TestCase("  // 123\n// abc\n foo", ExpectedResult = "foo")]
  [TestCase("/* 123 \n \n *  */foo", ExpectedResult = "foo")]
  public string TestSkipCommentsAndWhitespace(string text) {
    using var tr = TextSchemaTestUtil.CreateTextReader(text);
    tr.SkipCommentsAndWhitespace();
    return tr.ReadRemainder();
  }
}