using NUnit.Framework;


namespace schema.binary.text {
  internal class StringLengthSourceGeneratorTests {
    [Test]
    public void TestImmediateLength() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ImmediateLengthWrapper : IBinaryConvertible {
    [StringLengthSource(SchemaIntegerType.UINT32)]
    public string Field { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Read(IEndianBinaryReader er) {
      {
        var l = er.ReadUInt32();
        this.Field = er.ReadString(l);
      }
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(this.Field.Length);
      ew.WriteString(this.Field);
    }
  }
}
");
    }
  }
}