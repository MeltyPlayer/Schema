using NUnit.Framework;


namespace schema.binary.attributes {
  internal class EndiannessGeneratorTests {
    [Test]
    public void TestNoEndianness() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class EndiannessWrapper : IBinaryConvertible {
    public uint Field { get; set; }
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Read(IBinaryReader br) {
      this.Field = br.ReadUInt32();
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteUInt32(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestEndiannessOnField() {
      BinarySchemaTestUtil.AssertGenerated(@"using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class EndiannessWrapper : IBinaryConvertible {

    public uint Field1 { get; set; }

    [Endianness(Endianness.BigEndian)]
    public uint Field2 { get; set; }

    public uint Field3 { get; set; }
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Read(IBinaryReader br) {
      this.Field1 = br.ReadUInt32();
      br.PushMemberEndianness(Endianness.BigEndian);
      this.Field2 = br.ReadUInt32();
      br.PopEndianness();
      this.Field3 = br.ReadUInt32();
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.WriteUInt32(this.Field1);
      bw.PushMemberEndianness(Endianness.BigEndian);
      bw.WriteUInt32(this.Field2);
      bw.PopEndianness();
      bw.WriteUInt32(this.Field3);
    }
  }
}
");
    }

    [Test]
    public void TestEndiannessOnClass() {
      BinarySchemaTestUtil.AssertGenerated(@"using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  [Endianness(Endianness.BigEndian)]
  public partial class EndiannessWrapper : IBinaryConvertible {
    public uint Field1 { get; set; }
    public uint Field2 { get; set; }
    public uint Field3 { get; set; }
  }
}",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Read(IBinaryReader br) {
      br.PushContainerEndianness(Endianness.BigEndian);
      this.Field1 = br.ReadUInt32();
      this.Field2 = br.ReadUInt32();
      this.Field3 = br.ReadUInt32();
      br.PopEndianness();
    }
  }
}
",
                                     @"using System;
using schema.binary;

namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Write(ISubBinaryWriter bw) {
      bw.PushContainerEndianness(Endianness.BigEndian);
      bw.WriteUInt32(this.Field1);
      bw.WriteUInt32(this.Field2);
      bw.WriteUInt32(this.Field3);
      bw.PopEndianness();
    }
  }
}
");
    }
  }
}