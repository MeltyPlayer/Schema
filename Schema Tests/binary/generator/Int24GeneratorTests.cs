using NUnit.Framework;


namespace schema.binary.text {
  internal class Int24GeneratorTests {
    [Test]
    public void TestInt24() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class Int24Wrapper {
    public void Read(IEndianBinaryReader er) {
      this.field1 = er.ReadInt24();
      er.AssertInt24(this.field2);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class Int24Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt24(this.field1);
      ew.WriteInt24(this.field2);
    }
  }
}
");
    }

    [Test]
    public void TestUInt24() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class UInt24Wrapper {
    public void Read(IEndianBinaryReader er) {
      this.field1 = er.ReadUInt24();
      er.AssertUInt24(this.field2);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class UInt24Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt24(this.field1);
      ew.WriteUInt24(this.field2);
    }
  }
}
");
    }
  }
}