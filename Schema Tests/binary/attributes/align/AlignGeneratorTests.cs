using NUnit.Framework;


namespace schema.binary.attributes.align {
  internal class AlignGeneratorTests {
    [Test] public void TestAlign() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.align;

namespace foo.bar {
  [BinarySchema]
  public partial class AlignWrapper : IBinaryConvertible {
    [Align(0x2)]
    public byte Field { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class AlignWrapper {
    public void Read(IEndianBinaryReader er) {
      er.Align(2);
      this.Field = er.ReadByte();
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class AlignWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.Align(2);
      ew.WriteByte(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestAlignWithImmediate() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.align;
using schema.binary.attributes.sequence;

namespace foo.bar {
  [BinarySchema]
  public partial class AlignWrapper : IBinaryConvertible {
    [Align(0x2)]
    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public int[] Field { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class AlignWrapper {
    public void Read(IEndianBinaryReader er) {
      {
        var c = er.ReadUInt32();
        if (c < 0) {
          throw new Exception(""Expected length to be nonnegative!"");
        }
        this.Field = new int[c];
      }
      er.Align(2);
      er.ReadInt32s(this.Field);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class AlignWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32((uint) this.Field.Length);
      ew.Align(2);
      ew.WriteInt32s(this.Field);
    }
  }
}
");
    }
  }
}