using NUnit.Framework;


namespace schema.binary.attributes {
  internal class WLengthOfSequenceAttributeTests {
    [Test]
    public void TestAttribute() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class SequenceWrapper : IBinaryConvertible {
    [WLengthOfSequence(nameof(Sequence)]
    public uint Length { get; private set; }

    [RSequenceLengthSource(nameof(Length))]
    public byte[] Sequence { get; set; }
  }
}",
                                           @"using System;
using System.IO;
using schema.util.sequences;

namespace foo.bar {
  public partial class SequenceWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Length = er.ReadUInt32();
      this.Sequence = SequencesUtil.CloneAndResizeSequence(this.Sequence, (int) this.Length);
      er.ReadBytes(this.Sequence);
    }
  }
}
",
                                           @"using System;
using System.IO;
namespace foo.bar {
  public partial class SequenceWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(Sequence.Length);
      ew.WriteBytes(this.Sequence);
    }
  }
}
");
    }
  }
}