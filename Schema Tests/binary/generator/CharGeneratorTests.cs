using NUnit.Framework;


namespace schema.binary.text;

internal class CharGeneratorTests {
  [Test]
  public void TestChar() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper {
    public char Field { get; set; }

    public char ReadonlyField { get; }
  }
}",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadChar();
      br.AssertChar(this.ReadonlyField);
    }
  }
}
",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteChar(this.Field);
      bw.WriteChar(this.ReadonlyField);
    }
  }
}
");
  }


  [Test]
  public void TestCharArray() {
    BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper {
    [SequenceLengthSource(4)]
    public char[] Field { get; set; }
  }
}",
                                         @"using System;
using schema.binary;
using schema.util.sequences;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IBinaryReader br) {
      this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 4);
      br.ReadChars(this.Field);
    }
  }
}
",
                                         @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteChars(this.Field);
    }
  }
}
");
  }
}