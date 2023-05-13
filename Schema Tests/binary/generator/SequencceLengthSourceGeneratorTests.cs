using NUnit.Framework;


namespace schema.binary.text {
  internal class SequencceLengthSourceGeneratorTests {
    [Test]
    public void TestConstLength() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.Collections.Generic;

using schema.binary;
using schema.binary.attributes.ignore;
using schema.binary.attributes.sequence;

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
namespace foo.bar {
  public partial class ConstLengthWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = new int[3];
      er.ReadInt32s(this.Field);
      this.NullableField = new int[3];
      er.ReadInt32s(this.NullableField);
      if (this.Toggle) {
        this.IfBooleanArray = new int[3];
        er.ReadInt32s(this.IfBooleanArray);
      }
      else {
        this.IfBooleanArray = null;
      }
      if (this.Toggle) {
        this.IfBooleanList = new System.Collections.Generic.List();
        while (this.IfBooleanList.Count < 3) {
          this.IfBooleanList.Add(default);
        }
        while (this.IfBooleanList.Count > 3) {
          this.IfBooleanList.RemoveAt(0);
        }
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
  }
}