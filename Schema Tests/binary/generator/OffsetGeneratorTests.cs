using NUnit.Framework;


namespace schema.binary.text {
  internal class OffsetGeneratorTests {
    [Test] public void TestOffset() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.offset;

namespace foo.bar {
  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible {
    public uint BaseLocation { get; set; }

    public uint Offset { get; set; }

    [Offset(nameof(BaseLocation), nameof(Offset))]
    public byte Field { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class OffsetWrapper {
    public void Read(IEndianBinaryReader er) {
      this.BaseLocation = er.ReadUInt32();
      this.Offset = er.ReadUInt32();
      {
        var tempLocation = er.Position;
        er.Position = this.BaseLocation + this.Offset;
        this.Field = er.ReadByte();
        er.Position = tempLocation;
      }
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class OffsetWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(this.BaseLocation);
      ew.WriteUInt32(this.Offset);
      throw new NotImplementedException();
    }
  }
}
");
    }

    [Test]
    public void TestOffsetFromParent() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.child_of;
using schema.binary.attributes.offset;

namespace foo.bar {
  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible, IChildOf<Parent> {
    public Parent Parent { get; set; }

    public uint Offset { get; set; }

    [Offset($""{nameof(Parent)}.{nameof(Parent.BaseLocation)}"", nameof(Offset))]
    public byte Field { get; set; }
  }

  public partial class Parent : IBinaryConvertible {
    public OffsetWrapper Child { get; set; }

    public uint BaseLocation { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class OffsetWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Offset = er.ReadUInt32();
      {
        var tempLocation = er.Position;
        er.Position = this.Parent.BaseLocation + this.Offset;
        this.Field = er.ReadByte();
        er.Position = tempLocation;
      }
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class OffsetWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(this.Offset);
      throw new NotImplementedException();
    }
  }
}
");
    }
  }
}