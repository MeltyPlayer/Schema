using System;
using System.Collections.Generic;

using schema.util.enumerables;


namespace schema.util.strings {
  public static class StringExtensions {
    public static IEnumerable<string> SplitViaChar(
        this string text,
        char separator,
        bool includeEmpty) {
      var list = new LinkedList<string>();

      var tSpan = text.AsSpan();
      var startIndex = 0;

      for (var i = 0; i < text.Length; ++i) {
        if (text[i] == separator) {
          var len = i - startIndex;
          if (includeEmpty || len > 0) {
            list.AddLast(tSpan.Slice(startIndex, len).ToString());
          }

          startIndex = i + 1;
        }
      }

      var lastLen = text.Length - startIndex;
      if (includeEmpty || lastLen > 0) {
        list.AddLast(tSpan.Slice(startIndex, lastLen).ToString());
      }

      return list;
    }

    public static IEnumerable<string> SplitViaChar(
        this string text,
        ReadOnlySpan<char> separators,
        bool includeEmpty) {
      var list = new LinkedList<string>();

      var tSpan = text.AsSpan();
      var startIndex = 0;

      for (var i = 0; i < text.Length; ++i) {
        var didMatch = false;

        for (var s = 0; s < separators.Length; ++s) {
          if (text[i] == separators[s]) {
            didMatch = true;
            break;
          }
        }

        if (didMatch) {
          var len = i - startIndex;
          if (includeEmpty || len > 0) {
            list.AddLast(tSpan.Slice(startIndex, len).ToString());
          }

          startIndex = i + 1;
        }
      }

      var lastLen = text.Length - startIndex;
      if (includeEmpty || lastLen > 0) {
        list.AddLast(tSpan.Slice(startIndex, lastLen).ToString());
      }

      return list;
    }


    public static IEnumerable<string> SplitViaString(
        this string text,
        string separator,
        bool includeEmpty) {
      if (separator.Length == 0) {
        return text.Yield();
      }

      var list = new LinkedList<string>();

      var tSpan = text.AsSpan();
      var startIndex = 0;

      var separatorLength = separator.Length;
      for (var i = 0; i < text.Length - (separatorLength - 1); ++i) {
        for (var si = 0; si < separatorLength; ++si) {
          if (text[i + si] != separator[si]) {
            goto DidNotMatchSeparator;
          }
        }

        // If made it here, then we did match
        var len = i - startIndex;
        if (includeEmpty || len > 0) {
          list.AddLast(tSpan.Slice(startIndex, len).ToString());
        }

        i += Math.Max(0, separatorLength - 1);
        startIndex = i + 1;

        DidNotMatchSeparator: ;
      }

      var lastLen = text.Length - startIndex;
      if (includeEmpty || lastLen > 0) {
        list.AddLast(tSpan.Slice(startIndex, lastLen).ToString());
      }

      return list;
    }


    public static IEnumerable<string> SplitViaString(
        this string text,
        ReadOnlySpan<string> separators,
        bool includeEmpty) {
      var list = new LinkedList<string>();

      var tSpan = text.AsSpan();
      var startIndex = 0;

      for (var i = 0; i < text.Length; ++i) {
        var didMatch = false;
        var longestMatchLength = 0;

        for (var s = 0; s < separators.Length; ++s) {
          var separator = separators[s];
          if (separator.Length == 0) {
            continue;
          }

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
          var len = i - startIndex;
          if (includeEmpty || len > 0) {
            list.AddLast(tSpan.Slice(startIndex, len).ToString());
          }

          i += Math.Max(0, longestMatchLength - 1);
          startIndex = i + 1;
        }
      }

      var lastLen = text.Length - startIndex;
      if (includeEmpty || lastLen > 0) {
        list.AddLast(tSpan.Slice(startIndex, lastLen).ToString());
      }

      return list;
    }
  }
}