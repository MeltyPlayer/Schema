using NUnit.Framework;


namespace schema.binary.text {
  internal class EnumGeneratorTests {
    [Test]
    public void TestEnum() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  enum A {}

  enum B : int {
  }
 
  [BinarySchema]
  public partial class EnumWrapper {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public A fieldA;

    public B fieldB;
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class EnumWrapper {
    public void Read(IEndianBinaryReader er) {
      this.fieldA = (A) er.ReadByte();
      this.fieldB = (B) er.ReadInt32();
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class EnumWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte((byte) this.fieldA);
      ew.WriteInt32((int) this.fieldB);
    }
  }
}
");
    }

    [Test]
    public void TestEnumArray() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  enum A {}

  enum B : int {
  }
 
  [BinarySchema]
  public partial class EnumWrapper {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public readonly A[] fieldA = new A[5];

    public readonly B[] fieldB = new B[5];
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class EnumWrapper {
    public void Read(IEndianBinaryReader er) {
      for (var i = 0; i < this.fieldA.Length; ++i) {
        this.fieldA[i] = (A) er.ReadByte();
      }
      for (var i = 0; i < this.fieldB.Length; ++i) {
        this.fieldB[i] = (B) er.ReadInt32();
      }
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class EnumWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      for (var i = 0; i < this.fieldA.Length; ++i) {
        ew.WriteByte((byte) this.fieldA[i]);
      }
      for (var i = 0; i < this.fieldB.Length; ++i) {
        ew.WriteInt32((int) this.fieldB[i]);
      }
    }
  }
}
");
    }
  }
}