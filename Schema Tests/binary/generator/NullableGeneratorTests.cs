using NUnit.Framework;


namespace schema.binary.text {
  internal class NullableGeneratorTests {
    [Test]
    public void TestNullable() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class NullableWrapper : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool? Field1 { get; set; }
    public int? Field2 { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class NullableWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field1 = er.ReadByte() != 0;
      this.Field2 = er.ReadInt32();
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class NullableWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte((byte) (this.Field1.Value ? 1 : 0));
      ew.WriteInt32(this.Field2.Value);
    }
  }
}
");
    }
  }
}