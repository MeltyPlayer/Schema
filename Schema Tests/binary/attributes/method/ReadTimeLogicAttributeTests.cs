using NUnit.Framework;


namespace schema.binary.attributes {
  internal class ReadTimeLogicAttributeTests {
    [Test]
    public void TestAttribute() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.IO;
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper : IBinaryConvertible {
    public byte Field1 { get; set; }

    [ReadTimeLogic]
    public void Method(IEndianBinaryReader er) {}

    public byte Field2 { get; set; }
  }
}",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field1 = er.ReadByte();
      this.Method(er);
      this.Field2 = er.ReadByte();
    }
  }
}
",
                                           @"using System;
using System.IO;
namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte(this.Field1);
      ew.WriteByte(this.Field2);
    }
  }
}
");
    }
  }
}