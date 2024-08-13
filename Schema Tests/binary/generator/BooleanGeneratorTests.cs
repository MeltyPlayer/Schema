using NUnit.Framework;


namespace schema.binary.text;

internal class BooleanGeneratorTests {
  [Test]
  public void TestByte() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool Field { get; set; }

    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool ReadonlyField { get; }
  }
}",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadByte() != 0;
      br.AssertByte((byte) (this.ReadonlyField ? 1 : 0));
    }
  }
}
",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteByte((byte) (this.Field ? 1 : 0));
      bw.WriteByte((byte) (this.ReadonlyField ? 1 : 0));
    }
  }
}
");
  }


  [Test]
  public void TestInt16() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool Field { get; set; }

    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool ReadonlyField { get; }
  }
}",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadInt16() != 0;
      br.AssertInt16((short) (this.ReadonlyField ? 1 : 0));
    }
  }
}
",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt16((short) (this.Field ? 1 : 0));
      bw.WriteInt16((short) (this.ReadonlyField ? 1 : 0));
    }
  }
}
");
  }


  [Test]
  public void TestInt32() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    [IntegerFormat(SchemaIntegerType.INT32)]
    public bool Field { get; set; }

    [IntegerFormat(SchemaIntegerType.INT32)]
    public bool ReadonlyField { get; }
  }
}",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadInt32() != 0;
      br.AssertInt32(this.ReadonlyField ? 1 : 0);
    }
  }
}
",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt32(this.Field ? 1 : 0);
      bw.WriteInt32(this.ReadonlyField ? 1 : 0);
    }
  }
}
");
  }


  [Test]
  public void TestByteArray() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    [SequenceLengthSource(4)]
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool[] Field { get; set; }
  }
}",
                                         @"using System;
using schema.binary;
using schema.util.sequences;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IBinaryReader br) {
      this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 4);
      for (var i = 0; i < this.Field.Length; ++i) {
        this.Field[i] = br.ReadByte() != 0;
      }
    }
  }
}
",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(IBinaryWriter bw) {
      for (var i = 0; i < this.Field.Length; ++i) {
        bw.WriteByte((byte) (this.Field[i] ? 1 : 0));
      }
    }
  }
}
");
  }
}