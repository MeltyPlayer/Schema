using NUnit.Framework;


namespace schema.binary.text {
  internal class EnumGeneratorTests {
    [Test]
    public void TestEnum() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

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
using schema.binary;

namespace foo.bar {
  public partial class EnumWrapper {
    public void Read(IBinaryReader br) {
      this.fieldA = (A) br.ReadByte();
      this.fieldB = (B) br.ReadInt32();
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class EnumWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteByte((byte) this.fieldA);
      bw.WriteInt32((int) this.fieldB);
    }
  }
}
");
    }

    [Test]
    public void TestEnumArray() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

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
using schema.binary;

namespace foo.bar {
  public partial class EnumWrapper {
    public void Read(IBinaryReader br) {
      for (var i = 0; i < this.fieldA.Length; ++i) {
        this.fieldA[i] = (A) br.ReadByte();
      }
      for (var i = 0; i < this.fieldB.Length; ++i) {
        this.fieldB[i] = (B) br.ReadInt32();
      }
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class EnumWrapper {
    public void Write(ISubBinaryWriter bw) {
      for (var i = 0; i < this.fieldA.Length; ++i) {
        bw.WriteByte((byte) this.fieldA[i]);
      }
      for (var i = 0; i < this.fieldB.Length; ++i) {
        bw.WriteInt32((int) this.fieldB[i]);
      }
    }
  }
}
");
    }
  }
}