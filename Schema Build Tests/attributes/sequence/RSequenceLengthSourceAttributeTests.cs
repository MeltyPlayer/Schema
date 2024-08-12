using System.Collections.Generic;

using schema.binary;
using schema.binary.attributes;


namespace build.attributes.sequence {
  public partial class RSequenceLengthSourceAttributeTests {
    [BinarySchema]
    public partial class ReadonlyListClass : IBinaryConvertible {
      [WLengthOfSequence(nameof(Values))]
      private uint count_;

      [RSequenceLengthSource(nameof(count_))]
      public readonly List<int> Values = new();
    }
  }
}