using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace schema.util.types;

public static class TypeExtensions {
  extension<T>(T) {
    public static IEnumerable<Type> AllImplementations()
      => Assembly.GetExecutingAssembly()
                 .GetTypes()
                 .Where(t => typeof(T).IsAssignableFrom(t) &&
                             !t.IsInterface &&
                             !t.IsAbstract);
  }

  public static string GetCorrectName(this Type type)
    => type.Name.Contains("`")
        ? type.Name.Substring(0, type.Name.IndexOf('`'))
        : type.Name;
}