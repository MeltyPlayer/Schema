﻿using NUnit.Framework;


namespace schema.binary.text {
  internal class NullableGeneratorTests {
    [Test]
    public void TestNullable() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class NullableWrapper : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.BYTE)]
    public bool? Field1 { get; set; }
    public int? Field2 { get; set; }
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class NullableWrapper {
    public void Read(IBinaryReader br) {
      this.Field1 = br.ReadByte() != 0;
      this.Field2 = br.ReadInt32();
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class NullableWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteByte((byte) (this.Field1.Value ? 1 : 0));
      bw.WriteInt32(this.Field2.Value);
    }
  }
}
");
    }
  }
}