using System;
using System.IO;


namespace schema.binary.attributes.endianness {
  /// <summary>
  ///   Attribute for specifying the endianness of a given class or field.
  ///
  ///   <para>
  ///     When added to a class, this will be used unless a different
  ///     endianness was already specified for the field, for the overall
  ///     reader/writer.
  ///   </para>
  ///   <para>
  ///     When added to a field, this take precedence over any preexisting
  ///     field, reader/writer, or class endianness.
  ///   </para>
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
                  AttributeTargets.Field | AttributeTargets.Property)]
  public class EndiannessAttribute : Attribute {
    public EndiannessAttribute(Endianness endianness) {
      this.Endianness = endianness;
    }

    public Endianness Endianness { get; }
  }
}