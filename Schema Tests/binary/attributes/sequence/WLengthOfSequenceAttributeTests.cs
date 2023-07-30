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
using System.IO;
using schema.util.sequences;

namespace foo.bar {
  public partial class SequenceWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Length = er.ReadInt32();
      this.Sequence1 = SequencesUtil.CloneAndResizeSequence(this.Sequence1, this.Length);
      er.ReadBytes(this.Sequence1);
      SequencesUtil.ResizeSequenceInPlace(this.Sequence2, this.Length);
      er.ReadBytes(this.Sequence2);
    }
  }
}
",
                                           @"using System;
using System.IO;
using schema.util;

namespace foo.bar {
  public partial class SequenceWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      Asserts.AllEqual(Sequence1.Length, Sequence2.Count);
      ew.WriteInt32(Sequence1.Length);
      ew.WriteBytes(this.Sequence1);
      ew.WriteBytes(this.Sequence2);
    }
  }
}
");
    }

  }
}