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
    public int Length { get; private set; }

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
      this.Length = er.ReadInt32();
      this.Sequence = SequencesUtil.CloneAndResizeSequence(this.Sequence, this.Length);
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
      ew.WriteInt32(Sequence.Length);
      ew.WriteBytes(this.Sequence);
    }
  }
}
");
    }

    [Test]
    public void TestAttributeWithSmallerThanInt() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class SequenceWrapper : IBinaryConvertible {
    [WLengthOfSequence(nameof(Sequence)]
    public ushort Length { get; private set; }

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
      this.Length = er.ReadUInt16();
      this.Sequence = SequencesUtil.CloneAndResizeSequence(this.Sequence, this.Length);
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
      ew.WriteUInt16((ushort) Sequence.Length);
      ew.WriteBytes(this.Sequence);
    }
  }
}
");
    }
  }
}