using NUnit.Framework;


namespace schema.binary.attributes {
  internal class RSequenceUntilEndOfStreamAttributeTests {
    [Test]
    public void TestByteArrayUntilEndOfStream() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper : IBinaryConvertible {
    [RSequenceUntilEndOfStream]
    public byte[] Field { get; set; }
  }
}",
                                           @"using System;
using System.Collections.Generic;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadBytes(er.Length - er.Position);
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteBytes(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestByteListUntilEndOfStream() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.Collections.Generic;

using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper : IBinaryConvertible {
    [RSequenceUntilEndOfStream]
    public List<byte> Field { get; } = new();
  }
}",
                                           @"using System;
using System.Collections.Generic;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      {
        this.Field.Clear();
        while (!er.Eof) {
          this.Field.Add(er.ReadByte());
        }
      }
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      for (var i = 0; i < this.Field.Count; ++i) {
        ew.WriteByte(this.Field[i]);
      }
    }
  }
}
");
    }

    [Test]
    public void TestIntArrayUntilEndOfStream() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper : IBinaryConvertible {
    [RSequenceUntilEndOfStream]
    public int[] Field { get; set; }
  }
}",
                                           @"using System;
using System.Collections.Generic;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadInt32s((er.Length - er.Position) / 4);
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt32s(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestIntListUntilEndOfStream() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.Collections.Generic;

using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class Wrapper : IBinaryConvertible {
    [RSequenceUntilEndOfStream]
    public List<int> Field { get; } = new();
  }
}",
                                           @"using System;
using System.Collections.Generic;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IEndianBinaryReader er) {
      {
        this.Field.Clear();
        while (!er.Eof) {
          this.Field.Add(er.ReadInt32());
        }
      }
    }
  }
}
",
                                           @"using System;
using System.IO;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      for (var i = 0; i < this.Field.Count; ++i) {
        ew.WriteInt32(this.Field[i]);
      }
    }
  }
}
");
    }
  }
}