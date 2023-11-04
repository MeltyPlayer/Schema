using NUnit.Framework;


namespace schema.binary.text {
  internal class RAtPositionOrNullGeneratorTests {
    [Test]
    public void TestOffset() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible {
    public uint Offset { get; set; }

    [RAtPositionOrNull(nameof(Offset), 123)]
    public byte? Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Read(IBinaryReader br) {
      this.Offset = br.ReadUInt32();
      if (this.Offset == 123) {
        this.Field = null;
      }
      else {
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
      bw.WriteByte(this.Field.Value);
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

    [RAtPositionOrNull(nameof(Parent.Offset))]
    public byte? Field { get; set; }
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
      if (this.Parent.Offset == 0) {
        this.Field = null;
      }
      else {
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
      bw.WriteByte(this.Field.Value);
    }
  }
}
");
    }
  }
}