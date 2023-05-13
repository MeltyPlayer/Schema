using System;


namespace schema.binary.attributes.ignore {
  /// <summary>
  ///   Schema attribute for designating a member that should not be included when reading/writing.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class IgnoreAttribute : Attribute { }
}