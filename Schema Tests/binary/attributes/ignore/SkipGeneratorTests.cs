using NUnit.Framework;


namespace schema.binary.attributes {
  internal class SkipGeneratorTests {
    [Test]
    public void TestSkip() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class SkipWrapper : IBinaryConvertible {
    [Skip]
    public byte Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class SkipWrapper {
    public void Read(IBinaryReader br) {
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class SkipWrapper {
    public void Write(IBinaryWriter bw) {
    }
  }
}
");
    }
  }
}