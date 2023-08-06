using NUnit.Framework;


namespace schema.binary.attributes {
  internal class IgnoreGeneratorTests {
    [Test]
    public void TestIgnore() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class IgnoreWrapper : IBinaryConvertible {
    [Ignore]
    public byte Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class IgnoreWrapper {
    public void Read(IEndianBinaryReader er) {
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class IgnoreWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
    }
  }
}
");
    }
  }
}