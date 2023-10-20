using NUnit.Framework;


namespace schema.binary.text {
  internal class GenericGeneratorTests {
    [Test]
    public void Test1GenericArgumentClass() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class GenericWrapper<T> : IBinaryConvertible where T : IBinaryConvertible, new() {
    public T Data { get; } = new();
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Read(IBinaryReader br) {
      this.Data.Read(br);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Write(IBinaryWriter bw) {
      this.Data.Write(bw);
    }
  }
}
");
    }

    [Test]
    public void Test2GenericArgumentClass() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class GenericWrapper<T1, T2> : IBinaryConvertible 
      where T1 : IBinaryConvertible, new(),
      where T2 : IBinaryConvertible, new(){
    public T1 Data1 { get; } = new();
    public T2 Data2 { get; } = new();
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T1, T2> {
    public void Read(IBinaryReader br) {
      this.Data1.Read(br);
      this.Data2.Read(br);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T1, T2> {
    public void Write(IBinaryWriter bw) {
      this.Data1.Write(bw);
      this.Data2.Write(bw);
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
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Read(IBinaryReader br) {
      foreach (var e in this.Data) {
        e.Read(br);
      }
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Write(IBinaryWriter bw) {
      foreach (var e in this.Data) {
        e.Write(bw);
      }
    }
  }
}
");
    }

    [Test]
    public void TestCanIgnoreInvalidGenerics() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

namespace foo.bar {
  [BinarySchema]
  public partial class GenericWrapper<T> : IBinaryConvertible {
    [Ignore]
    public T[] Data { get; } = {};
  }
}",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Read(IBinaryReader br) {
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Write(IBinaryWriter bw) {
    }
  }
}
");
    }

    [Test]
    public void TestIgnoresIgnoredFieldsThatFailedToParse() {
      BinarySchemaTestUtil.AssertGenerated(@"
using schema.binary;
using schema.binary.attributes;

public interface IMagicSection<T> {
  T Data { get; }
}

public class MagicSectionStub<T> : IMagicSection<T>, IBinaryConvertible {
  public T Data { get; set; }
  public void Write(IBinaryWriter bw) { }
  public void Read(IBinaryReader br) { }
}

[BinarySchema]
public partial class SwitchMagicStringUInt32SizedSection<T> : IMagicSection<T>
    where T : IBinaryConvertible {
  [Ignore]
  private readonly int magicLength_;

  [Ignore]
  private readonly Func<string, T> createTypeHandler_;

  private readonly MagicSectionStub<T> impl_ = new();

  [Ignore]
  public T Data => this.impl_.Data;
}",
                                           @"using System;
using schema.binary;

public partial class SwitchMagicStringUInt32SizedSection<T> {
  public void Read(IBinaryReader br) {
    this.impl_.Read(br);
  }
}
",
                                           @"using System;
using schema.binary;

public partial class SwitchMagicStringUInt32SizedSection<T> {
  public void Write(IBinaryWriter bw) {
    this.impl_.Write(bw);
  }
}
");
    }
  }
}