using System;


namespace schema.binary.attributes {
  public enum StringEncodingType {
    ASCII,
    UTF8,
    UTF16,
    UTF32,
  }

  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class StringEncodingAttribute : BMemberAttribute<string> {
    public StringEncodingAttribute(StringEncodingType encodingType) {
      this.EncodingType = encodingType;
    }

    protected override void InitFields() { }

    public StringEncodingType EncodingType { get; private set; }
  }
}