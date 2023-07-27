using NUnit.Framework;


namespace schema.binary.attributes {
  internal class NullTerminatedStringAttributeTests {
    [Test]
    public void TestNullTerminatedString() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class NtsWrapper : IBinaryConvertible {
    [NullTerminatedString]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class NtsWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadStringNT();
    }
  }
}
",
                                           @"using System;
using System.IO;
namespace foo.bar {
  public partial class NtsWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteStringNT(this.Field);
    }
  }
}
");
    }
  }
}