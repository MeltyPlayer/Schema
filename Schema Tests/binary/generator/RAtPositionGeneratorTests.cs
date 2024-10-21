using NUnit.Framework;

using schema.binary.attributes;


namespace schema.binary.text;

internal class RAtPositionGeneratorTests {
  [Test]
  public void TestPrimitiveAtOffset() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible {
    public uint Offset { get; set; }

    [RAtPosition(nameof(Offset))]
    public byte Primitive { get; set; }
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
        this.Primitive = br.ReadByte();
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
      bw.WriteByte(this.Primitive);
    }
  }
}
");
  }

  [Test]
  public void TestClassAtOffset() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  public partial class A : IBinaryConvertible;

  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible {
    public uint Offset { get; set; }

    [RAtPosition(nameof(Offset))]
    public A Class { get; set; }
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
        this.Class.Read(br);
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
      this.Class.Write(bw);
    }
  }
}
");
  }

  [Test]
  public void TestStructAtOffset() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  public partial struct A : IBinaryConvertible;

  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible {
    public uint Offset { get; set; }

    [RAtPosition(nameof(Offset))]
    public A Struct { get; set; }
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
        {
          var value = this.Struct;
          value.Read(br);
          this.Struct = value;
        }
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
      this.Struct.Write(bw);
    }
  }
}
");
  }

  [Test]
  public void TestArrayAtOffset() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible {
    public uint Offset { get; set; }

    [RAtPosition(nameof(Offset))]
    [SequenceLengthSource((uint) 3)]
    public byte[] Array { get; set; }
  }
}",
                                         @"using System;
using schema.binary;
using schema.util.sequences;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Read(IBinaryReader br) {
      this.Offset = br.ReadUInt32();
      {
        var tempLocation = br.Position;
        br.Position = this.Offset;
        this.Array = SequencesUtil.CloneAndResizeSequence(this.Array, 3);
        br.ReadBytes(this.Array);
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
      bw.WriteBytes(this.Array);
    }
  }
}
");
  }

  [Test]
  public void TestImmediateArrayAtOffset() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible {
    public uint Offset { get; set; }

    [RAtPosition(nameof(Offset))]
    [SequenceLengthSource(SchemaIntegerType.UINT32)]
    public byte[] ImmediateArray { get; set; }
  }
}",
                                         @"using System;
using schema.binary;
using schema.util.sequences;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Read(IBinaryReader br) {
      this.Offset = br.ReadUInt32();
      {
        var tempLocation = br.Position;
        br.Position = this.Offset;
        {
          var c = br.ReadUInt32();
          this.ImmediateArray = SequencesUtil.CloneAndResizeSequence(this.ImmediateArray, (int) c);
        }
        br.ReadBytes(this.ImmediateArray);
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
      bw.WriteUInt32((uint) this.ImmediateArray.Length);
      bw.WriteBytes(this.ImmediateArray);
    }
  }
}
");
  }

  [Test]
  public void TestArrayUntilEndOfStreamAtOffset() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class OffsetWrapper : IBinaryConvertible {
    public uint Offset { get; set; }

    [RAtPosition(nameof(Offset))]
    [RSequenceUntilEndOfStream]
    public byte[] EndOfStreamArray { get; set; }
  }
}",
                                         @"using System;
using System.Collections.Generic;
using schema.binary;

namespace foo.bar {
  public partial class OffsetWrapper {
    public void Read(IBinaryReader br) {
      this.Offset = br.ReadUInt32();
      {
        var tempLocation = br.Position;
        br.Position = this.Offset;
        this.EndOfStreamArray = br.ReadBytes(br.Length - br.Position);
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
      bw.WriteBytes(this.EndOfStreamArray);
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