using NUnit.Framework;


namespace schema.binary.attributes {
  internal class NullTerminatedStringAttributeTests {
    [Test]
    public void TestNullTerminatedString() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class NtsWrapper : IBinaryConvertible {
    [NullTerminatedString]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadStringNT();
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteStringNT(this.Field);
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
    [NullTerminatedString]
    [StringLengthSource(16)]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadStringNT(16);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteStringWithExactLength(this.Field, 16);
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

    [NullTerminatedString]
    [RStringLengthSource(nameof(Length))]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Length = er.ReadUInt32();
      this.Field = er.ReadStringNT(Length);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(this.Length);
      ew.WriteString(this.Field);
    }
  }
}
");
    }
  }
}