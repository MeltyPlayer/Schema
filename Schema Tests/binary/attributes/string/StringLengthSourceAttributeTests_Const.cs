using NUnit.Framework;


namespace schema.binary.attributes {
  internal partial class StringLengthSourceAttributeTests {
    [Test]
    public void TestConstString() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class StringWrapper {
    public readonly string Field = ""foo"";
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class StringWrapper {
    public void Read(IBinaryReader br) {
      br.AssertString(this.Field);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class StringWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteString(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestConstLengthString() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class StringWrapper {
    [StringLengthSource(3)]
    public string Field;
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class StringWrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadString(3);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class StringWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteStringWithExactLength(this.Field, 3);
    }
  }
}
");
    }
  }
}