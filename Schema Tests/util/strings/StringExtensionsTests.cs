using NUnit.Framework;


namespace schema.util.strings;

public class StringExtensionsTests {
  [Test]
  [TestCase("foobar", ' ', false, new[] {"foobar"})]
  [TestCase("foobar", ' ', true, new[] {"foobar"})]
  [TestCase("  hello world  ", ' ', false, new[] {"hello", "world"})]
  [TestCase("  hello world  ",
            ' ',
            true,
            new[] {"", "", "hello", "world", "", ""})]
  public void TestSplitViaChar(string input,
                               char separator,
                               bool includeEmpty,
                               string[] expectedStrings) {
    CollectionAssert.AreEqual(expectedStrings,
                              input.SplitViaChar(separator, includeEmpty));
  }

  [Test]
  [TestCase("1\t2\n3", "\t\n", false, new[] {"1", "2", "3"})]
  [TestCase("1\t2\n3", "\t\n", true, new[] {"1", "2", "3"})]
  [TestCase("foobar", "", true, new[] {"foobar"})]
  [TestCase("\t\nhello world\t\n",
            " \t\n",
            false,
            new[] {"hello", "world"})]
  [TestCase("\t\nhello world\t\n",
            " \t\n",
            true,
            new[] {"", "", "hello", "world", "", ""})]
  public void TestSplitViaChars(string input,
                                string separators,
                                bool includeEmpty,
                                string[] expectedStrings) {
    CollectionAssert.AreEqual(expectedStrings,
                              input.SplitViaChar(separators, includeEmpty));
  }

  [Test]
  [TestCase("foo", "", true, new[] {"foo"})]
  [TestCase("foobar", " ", false, new[] {"foobar"})]
  [TestCase("foobar", " ", true, new[] {"foobar"})]
  [TestCase("  hello world  ", " ", false, new[] {"hello", "world"})]
  [TestCase("  hello world  ",
            " ",
            true,
            new[] {"", "", "hello", "world", "", ""})]
  [TestCase("foo123bar", "123", true, new[] {"foo", "bar"})]
  public void TestSplitViaString(string input,
                                 string separator,
                                 bool includeEmpty,
                                 string[] expectedStrings) {
    CollectionAssert.AreEqual(expectedStrings,
                              input.SplitViaString(separator, includeEmpty));
  }

  [Test]
  [TestCase("foo", new[] {""}, true, new[] {"foo"})]
  [TestCase("foobar", new[] {" "}, false, new[] {"foobar"})]
  [TestCase("foobar", new[] {" "}, true, new[] {"foobar"})]
  [TestCase("  hello world  ",
            new[] {" "},
            false,
            new[] {"hello", "world"})]
  [TestCase("  hello world  ",
            new[] {" "},
            true,
            new[] {"", "", "hello", "world", "", ""})]
  [TestCase("foo123bar", new[] {"123"}, true, new[] {"foo", "bar"})]
  [TestCase("\nline1\r\nline2\nline3\r\n",
            new[] {"\n", "\r\n"},
            false,
            new[] {"line1", "line2", "line3"})]
  [TestCase("\nline1\r\nline2\nline3\r\n",
            new[] {"\n", "\r\n"},
            true,
            new[] {"", "line1", "line2", "line3", ""})]
  public void TestSplitViaStrings(string input,
                                  string[] separators,
                                  bool includeEmpty,
                                  string[] expectedStrings) {
    CollectionAssert.AreEqual(expectedStrings,
                              input.SplitViaString(separators, includeEmpty));
  }
}