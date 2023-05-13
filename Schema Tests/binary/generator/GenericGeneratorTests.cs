using NUnit.Framework;


namespace schema.binary.text {
  internal class GenericGeneratorTests {
    [Test]
    public void TestGenericStructure() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class GenericWrapper<T> : IBinaryConvertible where T : IBinaryConvertible, new() {
    public T Data { get; } = new();
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Read(IEndianBinaryReader er) {
      this.Data.Read(er);
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Write(ISubEndianBinaryWriter ew) {
      this.Data.Write(ew);
    }
  }
}
");
    }

    [Test]
    public void TestGenericStructureArray() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class GenericWrapper<T> : IBinaryConvertible where T : IBinaryConvertible, new() {
    public T[] Data { get; } = {};
  }
}",
                                     @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Read(IEndianBinaryReader er) {
      foreach (var e in this.Data) {
        e.Read(er);
      }
    }
  }
}
",
                                     @"using System;
using System.IO;
namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Write(ISubEndianBinaryWriter ew) {
      foreach (var e in this.Data) {
        e.Write(ew);
      }
    }
  }
}
");
    }
  }
}