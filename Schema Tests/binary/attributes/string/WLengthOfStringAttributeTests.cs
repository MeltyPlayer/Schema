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
using schema.binary;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IBinaryReader br) {
      this.Length = br.ReadUInt32();
      this.Text = br.ReadString(Length);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteUInt32((uint) Text.Length);
      bw.WriteString(this.Text);
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
using schema.binary;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IBinaryReader br) {
      this.Length = br.ReadUInt32();
      this.Text1 = br.ReadString(Length);
      this.Text2 = br.ReadString(Length);
    }
  }
}
",
                                           @"using System;
using schema.binary;
using schema.util.asserts;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubBinaryWriter bw) {
      Asserts.AllEqual(Text1.Length, Text2.Length);
      bw.WriteUInt32((uint) Text1.Length);
      bw.WriteString(this.Text1);
      bw.WriteString(this.Text2);
    }
  }
}
");
    }
  }
}