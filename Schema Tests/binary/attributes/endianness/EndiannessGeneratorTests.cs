using NUnit.Framework;


namespace schema.binary.attributes.endianness {
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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field = er.ReadUInt32();
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestEndiannessOnField() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.IO;

using schema.binary;
using schema.binary.attributes.endianness;

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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Read(IEndianBinaryReader er) {
      this.Field1 = er.ReadUInt32();
      er.PushMemberEndianness(Endianness.BigEndian);
      this.Field2 = er.ReadUInt32();
      er.PopEndianness();
      this.Field3 = er.ReadUInt32();
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteUInt32(this.Field1);
      ew.PushMemberEndianness(Endianness.BigEndian);
      ew.WriteUInt32(this.Field2);
      ew.PopEndianness();
      ew.WriteUInt32(this.Field3);
    }
  }
}
");
    }

    [Test]
    public void TestEndiannessOnClass() {
      BinarySchemaTestUtil.AssertGenerated(@"
using System.IO;

using schema.binary;
using schema.binary.attributes.endianness;

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
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Read(IEndianBinaryReader er) {
      er.PushStructureEndianness(Endianness.BigEndian);
      this.Field1 = er.ReadUInt32();
      this.Field2 = er.ReadUInt32();
      this.Field3 = er.ReadUInt32();
      er.PopEndianness();
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class EndiannessWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.PushStructureEndianness(Endianness.BigEndian);
      ew.WriteUInt32(this.Field1);
      ew.WriteUInt32(this.Field2);
      ew.WriteUInt32(this.Field3);
      ew.PopEndianness();
    }
  }
}
");
    }
  }
}