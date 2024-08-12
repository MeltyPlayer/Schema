using NUnit.Framework;


namespace schema.binary.attributes {
  internal partial class StringLengthSourceAttributeTests {
    [Test]
    public void TestImmediateLengthUInt32() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ImmediateLengthWrapper : IBinaryConvertible {
    [StringLengthSource(SchemaIntegerType.UINT32)]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Read(IBinaryReader br) {
      {
        var l = br.ReadUInt32();
        this.Field = br.ReadString(l);
      }
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteUInt32((uint) this.Field.Length);
      bw.WriteString(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestImmediateLengthByte() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ImmediateLengthWrapper : IBinaryConvertible {
    [StringLengthSource(SchemaIntegerType.BYTE)]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Read(IBinaryReader br) {
      {
        var l = br.ReadByte();
        this.Field = br.ReadString(l);
      }
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteByte((byte) this.Field.Length);
      bw.WriteString(this.Field);
    }
  }
}
");
    }
  }
}