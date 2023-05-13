using System;
using System.Collections.Generic;
using System.Linq;


namespace schema.text {
  /// <summary>
  ///   Attribute for automatically generating Read/Write methods on
  ///   classes/structs. These are generated at compile-time, so the field
  ///   order will be 1:1 to the original class/struct and there should be no
  ///   performance cost compared to manually defined logic.
  ///
  ///   For any types that have this attribute, DO NOT modify or move around
  ///   the fields unless you know what you're doing!
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public class TextSchemaAttribute : Attribute { }


  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class ReadUpToAttribute : Attribute {
    public ReadUpToAttribute(char primary, params char[] secondary)
      => this.UpToStrings =
          new[] { primary }.Concat(secondary).Select(c => $"{c}").ToArray();

    public ReadUpToAttribute(string primary, params string[] secondary)
      => this.UpToStrings = new[] { primary }.Concat(secondary).ToArray();

    public IReadOnlyList<string> UpToStrings { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class ReadUpToRegexAttribute : Attribute {
    public ReadUpToRegexAttribute(char primary, params char[] secondary)
      => this.UpToRegexStrings =
          new[] { primary }.Concat(secondary).Select(c => $"{c}").ToArray();

    public ReadUpToRegexAttribute(string primary, params string[] secondary)
      => this.UpToRegexStrings = new[] { primary }.Concat(secondary).ToArray();

    public IReadOnlyList<string> UpToRegexStrings { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class ReadWhileAttribute : Attribute {
    public ReadWhileAttribute(char primary, params char[] secondary)
      => this.WhileStrings =
          new[] { primary }.Concat(secondary).Select(c => $"{c}").ToArray();

    public ReadWhileAttribute(string primary, params string[] secondary)
      => this.WhileStrings = new[] { primary }.Concat(secondary).ToArray();

    public IReadOnlyList<string> WhileStrings { get; }
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class ReadWhileRegexAttribute : Attribute {
    public ReadWhileRegexAttribute(char primary, params char[] secondary)
      => this.WhileRegexStrings =
          new[] { primary }.Concat(secondary).Select(c => $"{c}").ToArray();

    public ReadWhileRegexAttribute(string primary, params string[] secondary)
      => this.WhileRegexStrings = new[] { primary }.Concat(secondary).ToArray();

    public IReadOnlyList<string> WhileRegexStrings { get; }
  }
}