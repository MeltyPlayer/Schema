using NUnit.Framework;


namespace schema.binary.attributes.memory {
  internal class PointerToGeneratorTests {
    [Test]
    public void TestPointerToInStructure() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.memory;

namespace foo.bar {
  [BinarySchema]
  public partial class SizeWrapper : IBinaryConvertible {
    [WPointerTo(nameof(Foo)]
    public uint FooSize { get; set; }

    public byte Foo;
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class SizeWrapper {
    public void Read(IEndianBinaryReader er) {
      this.FooSize = er.ReadUInt32();
      this.Foo = er.ReadByte();
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class SizeWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32Delayed(ew.GetPointerToMemberRelativeToScope(""Foo"").ContinueWith(task => (uint) task.Result));
      ew.MarkStartOfMember(""Foo"");
      ew.WriteByte(this.Foo);
      ew.MarkEndOfMember();
    }
  }
}
");
    }

    [Test]
    public void TestPointerToThroughChild() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes.memory;

namespace foo.bar {
  [BinarySchema]
  public partial class SizeWrapper : IBinaryConvertible {
    [WPointerTo($""{nameof(Foo)}.{nameof(Foo.Bar}"")]
    public uint FooBarSize { get; set; }

    public Child Foo;
  }

  [BinarySchema]
  public partial class Child : IBinaryConvertible {
    public byte Bar;
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class SizeWrapper {
    public void Read(IEndianBinaryReader er) {
      this.FooBarSize = er.ReadUInt32();
      this.Foo.Read(er);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class SizeWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32Delayed(ew.GetPointerToMemberRelativeToScope(""Foo.Bar"").ContinueWith(task => (uint) task.Result));
      ew.MarkStartOfMember(""Foo"");
      this.Foo.Write(ew);
      ew.MarkEndOfMember();
    }
  }
}
");
    }

    [Test]
    public void TestPointerToThroughParent() {
      BinarySchemaTestUtil.AssertGeneratedForAll(@"
using schema.binary;
using schema.binary.attributes.child_of;
using schema.binary.attributes.memory;

namespace foo.bar {
  [BinarySchema]
  public partial class SizeWrapper : IChildOf<ParentImpl>, IBinaryConvertible {
    public ParentImpl Parent;

    [WPointerTo($""{nameof(Parent)}.{nameof(Parent.Foo}"")]
    public uint FooSize { get; set; }
  }

  [BinarySchema]
  public partial class ParentImpl : IBinaryConvertible {
    public SizeWrapper Child;

    public byte Foo;
  }
}",
// Size Wrapper                                           
                                           (@"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class SizeWrapper {
    public void Read(IEndianBinaryReader er) {
      this.FooSize = er.ReadUInt32();
    }
  }
}
",
                                            @"using System;
using System.IO;
namespace foo.bar {
  public partial class SizeWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32Delayed(ew.GetPointerToMemberRelativeToScope(""Foo"").ContinueWith(task => (uint) task.Result));
    }
  }
}
"),
// Parent Impl
                                           (@"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ParentImpl {
    public void Read(IEndianBinaryReader er) {
      this.Child.Parent = this;
      this.Child.Read(er);
      this.Foo = er.ReadByte();
    }
  }
}
",
                                            @"using System;
using System.IO;
namespace foo.bar {
  public partial class ParentImpl {
    public void Write(ISubEndianBinaryWriter ew) {
      this.Child.Write(ew);
      ew.MarkStartOfMember(""Foo"");
      ew.WriteByte(this.Foo);
      ew.MarkEndOfMember();
    }
  }
}
"));
    }
  }
}