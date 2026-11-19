using System;
using System.Collections.Generic;
using System.Text;

using schema.util.strings;

namespace schema.text.reader;

public static partial class TextReaderExtensions {
  public static string ReadWord(this ITextReader tr) {
    tr.SkipWhitespace();
    return tr.ReadUpToStartOfTerminator([
        " ", "\t", "\n", "\r\n", ",", "{", "[", "}", "]", ":"
    ]);
  }

  public static string ReadRemainder(this ITextReader tr) {
    var sb = new StringBuilder();
    while (!tr.Eof) {
      sb.Append(tr.ReadChar());
    }

    return sb.ToString();
  }

  public static string[] ReadArguments(
      this ITextReader tr,
      ReadOnlySpan<char> separators,
      ReadOnlySpan<char> terminators) {
    if (tr.Eof) {
      return [];
    }

    var arguments = new List<string>();

    using var sb = new ValueStringBuilder();
    using var internalWhitespaceSb = new ValueStringBuilder();

    char? inQuoteType = null;

    do {
      var c = tr.ReadChar();

      if (inQuoteType == null) {
        foreach (var terminatorC in terminators) {
          if (c == terminatorC) {
            --tr.Position;
            goto End;
          }
        }

        foreach (var separatorC in separators) {
          if (c == separatorC) {
            arguments.Add(sb.ToString());
            sb.Clear();
            internalWhitespaceSb.Clear();
            goto Next;
          }
        }

        if (c is ' ' or '\t' or '\n') {
          if (sb.Length > 0) {
            internalWhitespaceSb.Append(c);
          }

          continue;
        }
      }

      if (c is '`' or '\'' or '"') {
        if (inQuoteType == null) {
          inQuoteType = c;
        } else if (inQuoteType == c) {
          inQuoteType = null;
        }
      }

      if (internalWhitespaceSb.Length > 0) {
        sb.Append(internalWhitespaceSb.AsSpan());
        internalWhitespaceSb.Clear();
      }

      sb.Append(c);

      Next: ;
    } while (!tr.Eof);

    End:
    if (sb.Length > 0) {
      arguments.Add(sb.ToString());
    }

    return arguments.ToArray();
  }
}