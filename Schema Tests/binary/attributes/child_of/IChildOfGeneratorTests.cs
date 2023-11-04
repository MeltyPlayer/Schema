using NUnit.Framework;


namespace schema.binary.attributes {
  internal class IChildOfGeneratorTests {
    [Test]
    public void TestChild() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ChildOfWrapper : IBinaryConvertible, IChildOf<Parent> {
    public Parent Parent { get; set; }

    public byte Field { get; set; }
  }

  public partial class Parent : IBinaryConvertible {
    public ChildOfWrapper Child { get; set; }
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ChildOfWrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadByte();
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ChildOfWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteByte(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestParent() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Parent {
    public ChildOfWrapper Child { get; set; }
  }

  public partial class ChildOfWrapper : IBinaryConvertible, IChildOf<Parent> {
    public Parent Parent { get; set; }

    public byte Field { get; set; }
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Parent {
    public void Read(IBinaryReader br) {
      this.Child.Parent = this;
      this.Child.Read(br);
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Parent {
    public void Write(IBinaryWriter bw) {
      this.Child.Parent = this;
      this.Child.Write(bw);
    }
  }
}
");
    }

    [Test]
    public void TestChildInArray() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ChildOfWrapper : IBinaryConvertible, IChildOf<Parent> {
    public Parent Parent { get; set; }
  }

  public partial class Parent : IBinaryConvertible {
    public uint Length { get; set; }

    [ArrayLengthSource(nameof(Length))]
    public ChildOfWrapper[] Child { get; set; }
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ChildOfWrapper {
    public void Read(IBinaryReader br) {
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ChildOfWrapper {
    public void Write(IBinaryWriter bw) {
    }
  }
}
");
    }

    [Test]
    public void TestParentOfArray() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Parent {
    private uint Length { get; set; }

    [RSequenceLengthSource(nameof(Length))]
    public ChildOfWrapper[] Child { get; set; }
  }

  public partial class ChildOfWrapper : IBinaryConvertible, IChildOf<Parent> {
    public Parent Parent { get; set; }
  }
}",
                                     @"using System;
using schema.binary;
using schema.util.sequences;

namespace foo.bar {
  public partial class Parent {
    public void Read(IBinaryReader br) {
      this.Length = br.ReadUInt32();
      this.Child = SequencesUtil.CloneAndResizeSequence(this.Child, (int) this.Length);
      foreach (var e in this.Child) {
        e.Parent = this;
        e.Read(br);
      }
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Parent {
    public void Write(IBinaryWriter bw) {
      bw.WriteUInt32(this.Length);
      foreach (var e in this.Child) {
        e.Parent = this;
        e.Write(bw);
      }
    }
  }
}
");
    }
  }
}