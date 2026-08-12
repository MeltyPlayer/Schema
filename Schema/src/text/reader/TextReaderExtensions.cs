using System;
using System.Collections.Generic;
using System.Text;

using schema.util.symbols;

namespace schema.text.reader;

public static partial class TextReaderExtensions {
  public static string[] ReadArguments(
      this ITextReader tr,
      ReadOnlySpan<char> separators,
      ReadOnlySpan<char> terminators) {
    if (tr.Eof) {
      return [];
    }

    var arguments = new List<string>();

    var sb = new StringBuilder();
    var internalWhitespaceSb = new StringBuilder();

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
        sb.Append(internalWhitespaceSb);
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