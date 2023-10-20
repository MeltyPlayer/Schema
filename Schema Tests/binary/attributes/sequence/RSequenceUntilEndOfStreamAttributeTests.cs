using NUnit.Framework;


namespace schema.binary.attributes {
  internal class RSequenceLengthSourceAttributeTests {
    [Test]
    public void TestReadonlyList() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.Collections.Generic;
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ReadonlyListClass : IBinaryConvertible {
    [WLengthOfSequence(nameof(Values))]
    private uint count_;

    [RSequenceLengthSource(nameof(count_))]
    public readonly List<int> Values = new();
  }
}",
                                           @"using System;
using schema.binary;
using schema.util.sequences;

namespace foo.bar {
  public partial class ReadonlyListClass {
    public void Read(IBinaryReader br) {
      this.count_ = br.ReadUInt32();
      SequencesUtil.ResizeSequenceInPlace(this.Values, (int) this.count_);
      for (var i = 0; i < this.Values.Count; ++i) {
        this.Values[i] = br.ReadInt32();
      }
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ReadonlyListClass {
    public void Write(IBinaryWriter bw) {
      bw.WriteUInt32((uint) Values.Count);
      for (var i = 0; i < this.Values.Count; ++i) {
        bw.WriteInt32(this.Values[i]);
      }
    }
  }
}
");
    }
  }
}