using NUnit.Framework;


namespace schema.binary.attributes {
  internal class EncodedNullTerminatedStringAttributeTests {
    [Test]
    public void TestNullTerminatedString() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class NtsWrapper : IBinaryConvertible {
    [StringEncoding(StringEncodingType.UTF8)]
    [NullTerminatedString]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadStringNT(StringEncodingType.UTF8);
    }
  }
}
",
                                           @"using System;
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteStringNT(StringEncodingType.UTF8, this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestNullTerminatedStringWithMaxConstLength() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class NtsWrapper : IBinaryConvertible {
    [StringEncoding(StringEncodingType.UTF8)]
    [NullTerminatedString]
    [StringLengthSource(16)]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadStringNT(StringEncodingType.UTF8, 16);
    }
  }
}
",
                                           @"using System;
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteStringWithExactLength(StringEncodingType.UTF8, this.Field, 16);
    }
  }
}
");
    }

    [Test]
    public void TestNullTerminatedStringWithMaxOtherLength() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class NtsWrapper : IBinaryConvertible {
    public uint Length { get; private set; }

    [StringEncoding(StringEncodingType.UTF8)]
    [NullTerminatedString]
    [RStringLengthSource(nameof(Length))]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IBinaryReader br) {
      this.Length = br.ReadUInt32();
      this.Field = br.ReadStringNT(StringEncodingType.UTF8, Length);
    }
  }
}
",
                                           @"using System;
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteUInt32(this.Length);
      bw.WriteString(StringEncodingType.UTF8, this.Field);
    }
  }
}
");
    }
  }
}