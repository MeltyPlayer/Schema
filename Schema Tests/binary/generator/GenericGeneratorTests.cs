﻿using NUnit.Framework;


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
    public void Read(IEndianBinaryReader er) {
      this.Data.Read(er);
    }
  }
}
",
                                           @"using System;
using schema.binary;

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
    public void Read(IEndianBinaryReader er) {
      this.Data1.Read(er);
      this.Data2.Read(er);
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T1, T2> {
    public void Write(ISubEndianBinaryWriter ew) {
      this.Data1.Write(ew);
      this.Data2.Write(ew);
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
    public void Read(IEndianBinaryReader er) {
      foreach (var e in this.Data) {
        e.Read(er);
      }
    }
  }
}
",
                                           @"using System;
using schema.binary;

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
    public void Read(IEndianBinaryReader er) {
    }
  }
}
",
                                           @"using System;
using schema.binary;

namespace foo.bar {
  public partial class GenericWrapper<T> {
    public void Write(ISubEndianBinaryWriter ew) {
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
  public void Write(ISubEndianBinaryWriter ew) { }
  public void Read(IEndianBinaryReader er) { }
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
  public void Read(IEndianBinaryReader er) {
    this.impl_.Read(er);
  }
}
",
                                           @"using System;
using schema.binary;

public partial class SwitchMagicStringUInt32SizedSection<T> {
  public void Write(ISubEndianBinaryWriter ew) {
    this.impl_.Write(ew);
  }
}
");
    }
  }
}