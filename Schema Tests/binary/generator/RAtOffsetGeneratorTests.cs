using NUnit.Framework;


namespace schema.binary.text {
  internal class RAtPositionGeneratorTests {
    [Test] public void TestOffset() {
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
    public void Read(IEndianBinaryReader er) {
      this.Offset = er.ReadUInt32();
      {
        var tempLocation = er.Position;
        er.Position = this.Offset;
        this.Field = er.ReadByte();
        er.Position = tempLocation;
      }
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(this.Offset);
      ew.WriteByte(this.Field);
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
    public void Read(IEndianBinaryReader er) {
      {
        var tempLocation = er.Position;
        er.Position = this.Parent.Offset;
        this.Field = er.ReadByte();
        er.Position = tempLocation;
      }
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte(this.Field);
    }
  }
}
");
    }
  }
}