using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schema.util.strings {
  public static class StringExtensions {
    // TODO: How to optimize this??
    public static string[] SplitViaSpans(this string text,
                                         ReadOnlySpan<string> separators,
                                         bool includeEmpty) {
      var items = new LinkedList<string>();

      var currentItem = new StringBuilder();

      for (var i = 0; i < text.Length; ++i) {
        var didMatch = false;
        var longestMatchLength = 0;

        for (var s = 0; s < separators.Length; ++s) {
          var separator = separators[s];
          if (text.Length - i < separator.Length) {
            continue;
          }

          for (var si = 0; si < separator.Length; ++si) {
            if (text[i + si] != separator[si]) {
              goto DidNotMatchSeparator;
            }
          }

          didMatch = true;
          longestMatchLength = Math.Max(longestMatchLength, separator.Length);

          DidNotMatchSeparator: ;
        }

        if (didMatch) {
          if (includeEmpty || currentItem.Length > 0) {
            items.AddLast(currentItem.ToString());
          }

          currentItem.Clear();

          i += longestMatchLength - 1;
        } else {
          currentItem.Append(text[i]);
        }
      }

      if (includeEmpty || currentItem.Length > 0) {
        items.AddLast(currentItem.ToString());
      }

      return items.ToArray();
    }
  }
}