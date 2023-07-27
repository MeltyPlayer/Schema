namespace schema.util {
  public static class NameofUtil {
    public static string GetChainedAccessFromCallerArgumentExpression(
        string text) {
      var nameof = "nameof(";

      var textLength = text.Length;
      var lastChar = text[textLength - 1];

      if (text.StartsWith(nameof) && lastChar == ')') {
        var nameofLength = nameof.Length;
        return text.Substring(nameofLength, textLength - 1 - nameofLength);
      }

      if (text[0] == '"' && lastChar == '"') {
        return text.Substring(1, textLength - 2);
      }

      return text;
    }
  }
}