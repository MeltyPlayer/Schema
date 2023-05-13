using System;

namespace schema.binary.attributes.align {
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class AlignAttribute : Attribute {
    public AlignAttribute(int align) {
      this.Align = align;
    }

    public int Align { get; }
  }
}
