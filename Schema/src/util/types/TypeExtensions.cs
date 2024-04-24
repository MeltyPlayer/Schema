using System;

namespace schema.util.types {
  public static class TypeExtensions {
    public static string GetCorrectName(this Type type)
      => type.Name.Contains("`")
          ? type.Name.Substring(0, type.Name.IndexOf('`'))
          : type.Name;
  }
}