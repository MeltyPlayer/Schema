using NUnit.Framework;


namespace schema.binary.text {
  internal class BooleanGeneratorTests {
    [Test] public void TestByte() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadByte() != 0;
      er.AssertByte(this.ReadonlyField ? 1 : 0);
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
  }
}