using NUnit.Framework;


namespace schema.binary.attributes.ignore {
  internal class IgnoreGeneratorTests {
    [Test] public void TestIgnore() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.ignore;

namespace foo.bar {
  [BinarySchema]
  public partial class IgnoreWrapper : IBinaryConvertible {
    [Ignore]
    public byte Field { get; set; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class IgnoreWrapper {
    public void Read(IEndianBinaryReader er) {
    }
  }
}
",
                                     @"using System;
using System.IO;
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