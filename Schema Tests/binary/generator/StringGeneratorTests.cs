using NUnit.Framework;


namespace schema.binary.text {
  internal class StringGeneratorTests {
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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class StringWrapper {
    public void Read(IEndianBinaryReader er) {
      er.AssertString(this.Field);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class StringWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteString(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestConstLengthString() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class StringWrapper {
    [StringLengthSource(3)]
    public string Field;
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class StringWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadString(3);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class StringWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteStringWithExactLength(this.Field, 3);
    }
  }
}
");
    }
  }
}