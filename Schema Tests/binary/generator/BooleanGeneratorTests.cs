using NUnit.Framework;


namespace schema.binary.text {
  internal class BooleanGeneratorTests {
    [Test]
    public void TestByte() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool Field { get; set; }

    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool ReadonlyField { get; }
  }
}",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadByte() != 0;
      er.AssertByte((byte) (this.ReadonlyField ? 1 : 0));
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte((byte) (this.Field ? 1 : 0));
      ew.WriteByte((byte) (this.ReadonlyField ? 1 : 0));
    }
  }
}
");
    }


    [Test]
    public void TestInt16() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool Field { get; set; }

    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool ReadonlyField { get; }
  }
}",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadInt16() != 0;
      er.AssertInt16((short) (this.ReadonlyField ? 1 : 0));
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt16((short) (this.Field ? 1 : 0));
      ew.WriteInt16((short) (this.ReadonlyField ? 1 : 0));
    }
  }
}
");
    }


    [Test]
    public void TestInt32() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    [IntegerFormat(SchemaIntegerType.INT32)]
    public bool Field { get; set; }

    [IntegerFormat(SchemaIntegerType.INT32)]
    public bool ReadonlyField { get; }
  }
}",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadInt32() != 0;
      er.AssertInt32(this.ReadonlyField ? 1 : 0);
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt32(this.Field ? 1 : 0);
      ew.WriteInt32(this.ReadonlyField ? 1 : 0);
    }
  }
}
");
    }


    [Test]
    public void TestByteArray() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    [SequenceLengthSource(4)]
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool[] Field { get; set; }
  }
}",
                                           @"using System;
using System.IO;
using schema.util.sequences;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 4);
      for (var i = 0; i < this.Field.Length; ++i) {
        this.Field[i] = er.ReadByte() != 0;
      }
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      for (var i = 0; i < this.Field.Length; ++i) {
        ew.WriteByte((byte) (this.Field[i] ? 1 : 0));
      }
    }
  }
}
");
    }
  }
}