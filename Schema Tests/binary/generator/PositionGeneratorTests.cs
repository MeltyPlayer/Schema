using NUnit.Framework;


namespace schema.binary.text {
  internal class PositionGeneratorTests {
    [Test]
    public void TestPosition() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.position;

namespace foo.bar {
  [BinarySchema]
  public partial class PositionWrapper : IBinaryConvertible {
    [PositionRelativeToStream]
    public long Position { get; set; }

    public byte Value { get; set; }

    [PositionRelativeToStream]
    public long ExpectedPosition { get; }
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class PositionWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Position = er.Position;
      this.Value = er.ReadByte();
      er.AssertPosition(this.ExpectedPosition);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class PositionWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte(this.Value);
    }
  }
}
");
    }
  }
}