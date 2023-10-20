using NUnit.Framework;


namespace schema.binary.text {
  internal class HalfGeneratorTests {
    [Test]
    public void TestHalf() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class HalfWrapper {
    [NumberFormat(SchemaNumberType.HALF)]
    public float field1;

    [NumberFormat(SchemaNumberType.HALF)]
    public readonly float field2;

    [NumberFormat(SchemaNumberType.HALF)]
    public readonly float[] field3 = new float[5];
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class HalfWrapper {
    public void Read(IBinaryReader br) {
      this.field1 = (float) br.ReadHalf();
      br.AssertHalf((float) this.field2);
      for (var i = 0; i < this.field3.Length; ++i) {
        this.field3[i] = (float) br.ReadHalf();
      }
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class HalfWrapper {
    public void Write(IBinaryWriter bw) {
      bw.WriteHalf((float) this.field1);
      bw.WriteHalf((float) this.field2);
      for (var i = 0; i < this.field3.Length; ++i) {
        bw.WriteHalf((float) this.field3[i]);
      }
    }
  }
}
");
    }
  }
}