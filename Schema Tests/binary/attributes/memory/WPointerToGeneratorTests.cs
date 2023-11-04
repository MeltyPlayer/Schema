using NUnit.Framework;


namespace schema.binary.attributes {
  internal class WPointerToGeneratorTests {
    [Test]
    public void TestPointerToInStructure() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class SizeWrapper : IBinaryConvertible {
    [WPointerTo(nameof(Foo))]
    public uint FooOffset { get; set; }

    public byte Foo;
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class SizeWrapper {
    public void Read(IBinaryReader br) {
      this.FooOffset = br.ReadUInt32();
      this.Foo = br.ReadByte();
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class SizeWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteUInt32Delayed(bw.GetPointerToMemberRelativeToScope(""Foo"").ContinueWith(task => (uint) task.Result));
      bw.MarkStartOfMember(""Foo"");
      bw.WriteByte(this.Foo);
      bw.MarkEndOfMember();
    }
  }
}
");
    }

    [Test]
    public void TestPointerToThroughChild() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class SizeWrapper : IBinaryConvertible {
    [WPointerTo(nameof(Foo.Bar))]
    public uint FooBarOffset { get; set; }

    public Child Foo;
  }

  [BinarySchema]
  public partial class Child : IBinaryConvertible {
    public byte Bar;
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class SizeWrapper {
    public void Read(IBinaryReader br) {
      this.FooBarOffset = br.ReadUInt32();
      this.Foo.Read(br);
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class SizeWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteUInt32Delayed(bw.GetPointerToMemberRelativeToScope(""Foo.Bar"").ContinueWith(task => (uint) task.Result));
      bw.MarkStartOfMember(""Foo"");
      this.Foo.Write(bw);
      bw.MarkEndOfMember();
    }
  }
}
");
    }

    [Test]
    public void TestPointerToThroughParent() {
      BinarySchemaTestUtil.AssertGeneratedForAll(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class SizeWrapper : IChildOf<ParentImpl>, IBinaryConvertible {
    public ParentImpl Parent;

    [WPointerTo(nameof(Parent.Foo))]
    public uint FooOffset { get; set; }
  }

  [BinarySchema]
  public partial class ParentImpl : IBinaryConvertible {
    public SizeWrapper Child;

    public byte Foo;
  }
}",
// Size Wrapper                                           
                                           (@"using System;
using schema.binary;

namespace foo.bar {
  public partial class SizeWrapper {
    public void Read(IBinaryReader br) {
      this.FooOffset = br.ReadUInt32();
    }
  }
}
",
                                            @"using System;
using schema.binary;

namespace foo.bar {
  public partial class SizeWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteUInt32Delayed(bw.GetPointerToMemberRelativeToScope(""Foo"").ContinueWith(task => (uint) task.Result));
    }
  }
}
"),
// Parent Impl
                                           (@"using System;
using schema.binary;

namespace foo.bar {
  public partial class ParentImpl {
    public void Read(IBinaryReader br) {
      this.Child.Parent = this;
      this.Child.Read(br);
      this.Foo = br.ReadByte();
    }
  }
}
",
                                            @"using System;
using schema.binary;

namespace foo.bar {
  public partial class ParentImpl {
    public void Write(IBinaryWriter bw) {
      this.Child.Write(bw);
      bw.MarkStartOfMember(""Foo"");
      bw.WriteByte(this.Foo);
      bw.MarkEndOfMember();
    }
  }
}
"));
    }
  }
}