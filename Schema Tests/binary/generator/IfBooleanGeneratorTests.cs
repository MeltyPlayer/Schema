using NUnit.Framework;


namespace schema.binary.text {
  internal class IfBooleanGeneratorTests {
    [Test]
    public void TestIfBoolean() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [IfBoolean(SchemaIntegerType.BYTE)]
    public A? ImmediateValue { get; set; }

    [IntegerFormat(SchemaIntegerType.BYTE)]
    private bool Field { get; set; }

    [RIfBoolean(nameof(Field))]
    public int? OtherValue { get; set; }
  }

  public class A : IBinaryConvertible { }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IBinaryReader br) {
      {
        var b = br.ReadByte() != 0;
        if (b) {
          this.ImmediateValue = new A();
          this.ImmediateValue.Read(br);
        }
        else {
          this.ImmediateValue = null;
        }
      }
      this.Field = br.ReadByte() != 0;
      if (this.Field) {
        this.OtherValue = br.ReadInt32();
      }
      else {
        this.OtherValue = null;
      }
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteByte((byte) (this.ImmediateValue != null ? 1 : 0));
      if (this.ImmediateValue != null) {
        this.ImmediateValue.Write(bw);
      }
      bw.WriteByte((byte) (this.Field ? 1 : 0));
      if (this.Field) {
        bw.WriteInt32(this.OtherValue.Value);
      }
    }
  }
}
");
    }

    [Test]
    public void TestIfBooleanWithPrivateSetter() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool Field { get; private set; }

    [RIfBoolean(nameof(Field))]
    public int? OtherValue { get; set; }
  }

  public class A : IBinaryConvertible { }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadByte() != 0;
      if (this.Field) {
        this.OtherValue = br.ReadInt32();
      }
      else {
        this.OtherValue = null;
      }
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteByte((byte) (this.Field ? 1 : 0));
      if (this.Field) {
        bw.WriteInt32(this.OtherValue.Value);
      }
    }
  }
}
");
    }

    [Test]
    public void TestUsingBoolFromChild() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper : IBinaryConvertible {
    public Wrapper Field { get; set; }

    [RIfBoolean(nameof(Field.Bool))]
    public int? OtherValue { get; set; }
  }

  public class Wrapper : IBinaryConvertible {
    public bool Bool { get; private set; }
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IBinaryReader br) {
      this.Field.Read(br);
      if (this.Field.Bool) {
        this.OtherValue = br.ReadInt32();
      }
      else {
        this.OtherValue = null;
      }
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubBinaryWriter bw) {
      this.Field.Write(bw);
      if (this.Field.Bool) {
        bw.WriteInt32(this.OtherValue.Value);
      }
    }
  }
}
");
    }
  }
}