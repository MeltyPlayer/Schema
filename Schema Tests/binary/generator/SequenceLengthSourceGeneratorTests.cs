using NUnit.Framework;


namespace schema.binary.text {
  internal class SequenceLengthSourceGeneratorTests {
    [Test]
    public void TestConstLength() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.Collections.Generic;

using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ConstLengthWrapper : IBinaryConvertible {
    [SequenceLengthSource(3)]
    public int[] Field { get; set; }

    [SequenceLengthSource(3)]
    public int[]? NullableField { get; set; }

    [Ignore]
    private bool Toggle { get; set; }

    [RIfBoolean(nameof(Toggle))]
    [SequenceLengthSource(3)]
    public int[]? IfBooleanArray { get; set; }

    [RIfBoolean(nameof(Toggle))]
    [SequenceLengthSource(3)]
    public List<int>? IfBooleanList { get; set; }
  }
}",
                                           @"using System;
using System.Collections.Generic;
using System.IO;
using schema.util.sequences;

namespace foo.bar {
  public partial class ConstLengthWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 3);
      er.ReadInt32s(this.Field);
      this.NullableField = SequencesUtil.CloneAndResizeSequence(this.NullableField, 3);
      er.ReadInt32s(this.NullableField);
      if (this.Toggle) {
        this.IfBooleanArray = SequencesUtil.CloneAndResizeSequence(this.IfBooleanArray, 3);
        er.ReadInt32s(this.IfBooleanArray);
      }
      else {
        this.IfBooleanArray = null;
      }
      if (this.Toggle) {
        this.IfBooleanList = new System.Collections.Generic.List();
        SequencesUtil.ResizeSequenceInPlace(this.IfBooleanList, 3);
        er.ReadInt32s(this.IfBooleanList);
      }
      else {
        this.IfBooleanList = null;
      }
    }
  }
}
",
                                           @"using System;
using System.IO;
namespace foo.bar {
  public partial class ConstLengthWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt32s(this.Field);
      ew.WriteInt32s(this.NullableField);
      if (this.Toggle) {
        ew.WriteInt32s(this.IfBooleanArray);
      }
      if (this.Toggle) {
        ew.WriteInt32s(this.IfBooleanList);
      }
    }
  }
}
");
    }

    [Test]
    public void TestISequence() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;
using schema.util.sequences;

namespace foo.bar {
  [BinarySchema]
  public partial class ConstLengthWrapper : IBinaryConvertible {
    [SequenceLengthSource(3)]
    public SequenceImpl<int, int> Field { get; } = new();
  }

  public class SequenceImpl<T1, T2> : ISequence<SequenceImpl<(T1 First, T2 Second)>, (T1 First, T2 Second)> { 
  }
}",
                                           @"using System;
using System.IO;
using schema.util.sequences;

namespace foo.bar {
  public partial class ConstLengthWrapper {
    public void Read(IEndianBinaryReader er) {
      SequencesUtil.ResizeSequenceInPlace(this.Field, 3);
      this.Field.Read(er);
    }
  }
}
",
                                           @"using System;
using System.IO;
namespace foo.bar {
  public partial class ConstLengthWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      this.Field.Write(ew);
    }
  }
}
");
    }
  }
}