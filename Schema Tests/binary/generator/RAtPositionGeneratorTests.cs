using NUnit.Framework;


namespace schema.binary.text;

internal class RAtPositionGeneratorTests {
  [Test]
  public void TestOffset() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible {
    public uint Offset { get; set; }

    [RAtPosition(nameof(Offset))]
    public byte Field { get; set; }
  }
}",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Read(IBinaryReader br) {
      this.Offset = br.ReadUInt32();
      {
        var tempLocation = br.Position;
        br.Position = this.Offset;
        this.Field = br.ReadByte();
        br.Position = tempLocation;
      }
    }
  }
}
",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteUInt32(this.Offset);
      bw.WriteByte(this.Field);
    }
  }
}
");
  }

  [Test]
  public void TestOffsetFromParent() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible, IChildOf<Parent> {
    public Parent Parent { get; set; }

    [RAtPosition(nameof(Parent.Offset))]
    public byte Field { get; set; }
  }

  public partial class Parent : IBinaryConvertible {
    public uint Offset { get; set; }

    public OffsetWrapper Child { get; set; }
  }
}",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Read(IBinaryReader br) {
      {
        var tempLocation = br.Position;
        br.Position = this.Parent.Offset;
        this.Field = br.ReadByte();
        br.Position = tempLocation;
      }
    }
  }
}
",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteByte(this.Field);
    }
  }
}
");
  }
}