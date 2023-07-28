﻿using NUnit.Framework;


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
using System.IO;
using System.Text;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadStringNT(Encoding.UTF8);
    }
  }
}
",
                                           @"using System;
using System.IO;
using System.Text;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteStringNT(Encoding.UTF8, this.Field);
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
using System.IO;
using System.Text;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadStringNT(Encoding.UTF8, 16);
    }
  }
}
",
                                           @"using System;
using System.IO;
using System.Text;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteStringWithExactLength(Encoding.UTF8, this.Field, 16);
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
using System.IO;
using System.Text;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Length = er.ReadUInt32();
      this.Field = er.ReadStringNT(Encoding.UTF8, Length);
    }
  }
}
",
                                           @"using System;
using System.IO;
using System.Text;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(this.Length);
      ew.WriteString(Encoding.UTF8, this.Field);
    }
  }
}
");
    }
  }
}