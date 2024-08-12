using NUnit.Framework;


namespace schema.binary.text {
  internal class Int24GeneratorTests {
    [Test]
    public void TestInt24() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Int24Wrapper {
    [NumberFormat(SchemaNumberType.INT24)]
    public int field1;

    [NumberFormat(SchemaNumberType.INT24)]
    public readonly int field2;
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Int24Wrapper {
    public void Read(IBinaryReader br) {
      this.field1 = br.ReadInt24();
      br.AssertInt24(this.field2);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Int24Wrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt24(this.field1);
      bw.WriteInt24(this.field2);
    }
  }
}
");
    }

    [Test]
    public void TestUInt24() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class UInt24Wrapper {
    [NumberFormat(SchemaNumberType.UINT24)]
    public uint field1;

    [NumberFormat(SchemaNumberType.UINT24)]
    public readonly uint field2;
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class UInt24Wrapper {
    public void Read(IBinaryReader br) {
      this.field1 = br.ReadUInt24();
      br.AssertUInt24(this.field2);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class UInt24Wrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteUInt24(this.field1);
      bw.WriteUInt24(this.field2);
    }
  }
}
");
    }
  }
}