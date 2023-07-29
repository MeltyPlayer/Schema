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
      ew.WriteUInt32(Text.Length);
      ew.WriteString(this.Text);
    }
  }
}
");
    }
  }
}