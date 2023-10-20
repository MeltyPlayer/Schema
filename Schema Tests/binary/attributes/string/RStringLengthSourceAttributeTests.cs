using NUnit.Framework;


namespace schema.binary.attributes {
  internal class RStringLengthSourceAttributeTests {
    [Test]
    public void TestOther() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ImmediateLengthWrapper : IBinaryConvertible {
    [WLengthOfString(nameof(Field)]
    private int length;

    [RStringLengthSource(nameof(length)]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Read(IBinaryReader br) {
      this.length = br.ReadInt32();
      this.Field = br.ReadString(length);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt32(Field.Length);
      bw.WriteString(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestOtherViaThis() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ImmediateLengthWrapper : IBinaryConvertible {
    [WLengthOfString(nameof(this.Field)]
    private int length;

    [RStringLengthSource(nameof(this.length)]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Read(IBinaryReader br) {
      this.length = br.ReadInt32();
      this.Field = br.ReadString(length);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt32(Field.Length);
      bw.WriteString(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestOtherViaName() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ImmediateLengthWrapper : IBinaryConvertible {
    [WLengthOfString(nameof(ImmediateLengthWrapper.Field)]
    private int length;

    [RStringLengthSource(nameof(ImmediateLengthWrapper.length)]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Read(IBinaryReader br) {
      this.length = br.ReadInt32();
      this.Field = br.ReadString(length);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt32(Field.Length);
      bw.WriteString(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestOtherViaFullyQualifiedName() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ImmediateLengthWrapper : IBinaryConvertible {
    [WLengthOfString(nameof(foo.bar.ImmediateLengthWrapper.Field)]
    private int length;

    [RStringLengthSource(nameof(foo.bar.ImmediateLengthWrapper.length)]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Read(IBinaryReader br) {
      this.length = br.ReadInt32();
      this.Field = br.ReadString(length);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt32(Field.Length);
      bw.WriteString(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestOtherViaPartiallyQualifiedName() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class ImmediateLengthWrapper : IBinaryConvertible {
    [WLengthOfString(nameof(bar.ImmediateLengthWrapper.Field)]
    private int length;

    [RStringLengthSource(nameof(bar.ImmediateLengthWrapper.length)]
    public string Field { get; set; }
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Read(IBinaryReader br) {
      this.length = br.ReadInt32();
      this.Field = br.ReadString(length);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ImmediateLengthWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteInt32(Field.Length);
      bw.WriteString(this.Field);
    }
  }
}
");
    }
  }
}