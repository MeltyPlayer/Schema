using NUnit.Framework;


namespace schema.binary.text {
  internal class IChildOfGeneratorTests {
    [Test]
    public void TestChild() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.child_of;

namespace foo.bar {
  [BinarySchema]
  public partial class ChildOfWrapper : IBinaryConvertible, IChildOf<Parent> {
    public Parent Parent { get; set; }

    public byte Field { get; set; }
  }

  public partial class Parent {
    public ChildOfWrapper Child { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ChildOfWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadByte();
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class ChildOfWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestParent() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.child_of;

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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class Parent {
    public void Read(IEndianBinaryReader er) {
      this.Child.Parent = this;
      this.Child.Read(er);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class Parent {
    public void Write(ISubEndianBinaryWriter ew) {
      this.Child.Write(ew);
    }
  }
}
");
    }

    [Test]
    public void TestChildInArray() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.child_of;

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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ChildOfWrapper {
    public void Read(IEndianBinaryReader er) {
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class ChildOfWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
    }
  }
}
");
    }

    [Test]
    public void TestParentOfArray() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.child_of;
using schema.binary.attributes.sequence;

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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class Parent {
    public void Read(IEndianBinaryReader er) {
      this.Length = er.ReadUInt32();
      if (this.Length < 0) {
        throw new Exception(""Expected length to be nonnegative!"");
      }
      this.Child = new ChildOfWrapper[this.Length];
      for (var i = 0; i < this.Length; ++i) {
        this.Child[i] = new ChildOfWrapper();
      }
      foreach (var e in this.Child) {
        e.Parent = this;
        e.Read(er);
      }
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class Parent {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(this.Length);
      foreach (var e in this.Child) {
        e.Write(ew);
      }
    }
  }
}
");
    }
  }
}