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
using schema.binary;
using schema.util.sequences;

namespace foo.bar {
  public partial class SequenceWrapper {
    public void Read(IBinaryReader br) {
      this.Length = br.ReadInt32();
      this.Sequence = SequencesUtil.CloneAndResizeSequence(this.Sequence, this.Length);
      br.ReadBytes(this.Sequence);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class SequenceWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt32(Sequence.Length);
      bw.WriteBytes(this.Sequence);
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
using schema.binary;
using schema.util.sequences;

namespace foo.bar {
  public partial class SequenceWrapper {
    public void Read(IBinaryReader br) {
      this.Length = br.ReadUInt16();
      this.Sequence = SequencesUtil.CloneAndResizeSequence(this.Sequence, this.Length);
      br.ReadBytes(this.Sequence);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class SequenceWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteUInt16((ushort) Sequence.Length);
      bw.WriteBytes(this.Sequence);
    }
  }
}
");
    }

    [Test]
    public void TestMultiple() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.Collections.Generic;
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class SequenceWrapper : IBinaryConvertible {
    [WLengthOfSequence(nameof(Sequence1)]
    [WLengthOfSequence(nameof(Sequence2)]
    public int Length { get; private set; }

    [RSequenceLengthSource(nameof(Length))]
    public byte[] Sequence1 { get; set; }

    [RSequenceLengthSource(nameof(Length))]
    public List<byte> Sequence2 { get; set; }
  }
}",
                                           @"using System;
using schema.binary;
using schema.util.sequences;

namespace foo.bar {
  public partial class SequenceWrapper {
    public void Read(IBinaryReader br) {
      this.Length = br.ReadInt32();
      this.Sequence1 = SequencesUtil.CloneAndResizeSequence(this.Sequence1, this.Length);
      br.ReadBytes(this.Sequence1);
      SequencesUtil.ResizeSequenceInPlace(this.Sequence2, this.Length);
      for (var i = 0; i < this.Sequence2.Count; ++i) {
        this.Sequence2[i] = br.ReadByte();
      }
    }
  }
}
",
                                           @"using System;
using schema.binary;
using schema.util.asserts;

namespace foo.bar {
  public partial class SequenceWrapper {
    public void Write(IBinaryWriter bw) {
      Asserts.AllEqual(Sequence1.Length, Sequence2.Count);
      bw.WriteInt32(Sequence1.Length);
      bw.WriteBytes(this.Sequence1);
      for (var i = 0; i < this.Sequence2.Count; ++i) {
        bw.WriteByte(this.Sequence2[i]);
      }
    }
  }
}
");
    }

  }
}