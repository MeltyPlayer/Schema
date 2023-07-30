using NUnit.Framework;


namespace schema.binary.attributes {
  internal class WLengthOfStringAttributeTests {
    [Test]
    public void TestAttribute() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class NtsWrapper : IBinaryConvertible {
    [WLengthOfString(nameof(Text)]
    public uint Length { get; private set; }

    [RStringLengthSource(nameof(Length))]
    public string Text { get; set; }
  }
}",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Length = er.ReadUInt32();
      this.Text = er.ReadString(Length);
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32((uint) Text.Length);
      ew.WriteString(this.Text);
    }
  }
}
");
    }

    [Test]
    public void TestMultipleStrings() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class NtsWrapper : IBinaryConvertible {
    [WLengthOfString(nameof(Text1)]
    [WLengthOfString(nameof(Text2)]
    public uint Length { get; private set; }

    [RStringLengthSource(nameof(Length))]
    public string Text1 { get; set; }

    [RStringLengthSource(nameof(Length))]
    public string Text2 { get; set; }
  }
}",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Length = er.ReadUInt32();
      this.Text1 = er.ReadString(Length);
      this.Text2 = er.ReadString(Length);
    }
  }
}
",
                                           @"using System;
using System.IO;
using schema.util;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      Asserts.AllEqual(Text1.Length, Text2.Length);
      ew.WriteUInt32((uint) Text1.Length);
      ew.WriteString(this.Text1);
      ew.WriteString(this.Text2);
    }
  }
}
");
    }
  }
}