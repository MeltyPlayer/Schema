using NUnit.Framework;


namespace schema.binary.text {
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
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadChar();
      er.AssertChar(this.ReadonlyField);
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteChar(this.Field);
      ew.WriteChar(this.ReadonlyField);
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
using System.IO;
using schema.util.sequences;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = SequencesUtil.CloneAndResizeSequence(this.Field, 4);
      er.ReadChars(this.Field);
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteChars(this.Field);
    }
  }
}
");
    }
  }
}