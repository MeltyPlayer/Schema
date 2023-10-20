﻿using NUnit.Framework;


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
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadBytes(br.Length - br.Position);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteBytes(this.Field);
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
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IBinaryReader br) {
      {
        this.Field.Clear();
        while (!br.Eof) {
          this.Field.Add(br.ReadByte());
        }
      }
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(IBinaryWriter bw) {
      for (var i = 0; i < this.Field.Count; ++i) {
        bw.WriteByte(this.Field[i]);
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
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadInt32s((br.Length - br.Position) / 4);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt32s(this.Field);
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
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Read(IBinaryReader br) {
      {
        this.Field.Clear();
        while (!br.Eof) {
          this.Field.Add(br.ReadInt32());
        }
      }
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class Wrapper {
    public void Write(IBinaryWriter bw) {
      for (var i = 0; i < this.Field.Count; ++i) {
        bw.WriteInt32(this.Field[i]);
      }
    }
  }
}
");
    }
  }
}