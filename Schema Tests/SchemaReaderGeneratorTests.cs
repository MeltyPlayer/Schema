using NUnit.Framework;

namespace schema.binary.text {
  public class SchemaReaderGeneratorTests {
    [SetUp]
    public void Setup() {}

    [Test]
    public void TestByte() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    public byte Field { get; }
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IEndianBinaryReader er) {
      er.AssertByte(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestSByte() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class SByteWrapper {
    public sbyte Field { get; }
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class SByteWrapper {
    public void Read(IEndianBinaryReader er) {
      er.AssertSByte(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestInt16() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ShortWrapper {
    public short Field { get; }
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ShortWrapper {
    public void Read(IEndianBinaryReader er) {
      er.AssertInt16(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestConstArray() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ArrayWrapper {
    public readonly int[] field;
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ArrayWrapper {
    public void Read(IEndianBinaryReader er) {
      er.ReadInt32s(this.field);
    }
  }
}
");
    }

    [Test]
    public void TestArrayOtherMemberLength() {
      this.AssertGenerated_(@"
using schema.binary;
using schema.binary.attributes.sequence;

namespace foo.bar {
  [BinarySchema]
  public partial class ArrayWrapper {
    private int length;

    [RSequenceLengthSource(nameof(ArrayWrapper.length))]
    public int[] field;
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ArrayWrapper {
    public void Read(IEndianBinaryReader er) {
      this.length = er.ReadInt32();
      if (this.length < 0) {
        throw new Exception(""Expected length to be nonnegative!"");
      }
      this.field = new int[this.length];
      er.ReadInt32s(this.field);
    }
  }
}
");
    }

    [Test]
    public void TestNestedClass() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  static internal partial class Parent {
    protected partial class Middle {
      public enum ValueEnum {
        A, B
      }

      [BinarySchema]
      private partial class Wrapper {
        public int length;
        [IntegerFormat(SchemaIntegerType.INT32)]
        public ValueEnum value;
      }
    }
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  static internal partial class Parent {
    protected partial class Middle {
      private partial class Wrapper {
        public void Read(IEndianBinaryReader er) {
          this.length = er.ReadInt32();
          this.value = (Parent.Middle.ValueEnum) er.ReadInt32();
        }
      }
    }
  }
}
");
    }

    [Test]
    public void TestConstCharArray() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class CharWrapper {
    public char[] Array { get; }
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class CharWrapper {
    public void Read(IEndianBinaryReader er) {
      er.ReadChars(this.Array);
    }
  }
}
");
    }

    [Test]
    public void TestField() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ShortWrapper {
    public short Field { get; }
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ShortWrapper {
    public void Read(IEndianBinaryReader er) {
      er.AssertInt16(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestProperty() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public class ByteWrapper {
    public byte field { get; set; }
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IEndianBinaryReader er) {
      this.field = er.ReadByte();
    }
  }
}
");
    }

    [Test]
    public void TestReadonlyPrimitiveField() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public class ByteWrapper {
    public readonly byte field;
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IEndianBinaryReader er) {
      er.AssertByte(this.field);
    }
  }
}
");
    }

    [Test]
    public void TestReadonlyPrimitiveProperty() {
      this.AssertGenerated_(@"
using schema.binary;

namespace foo.bar {
  [BinarySchema]
  public partial class ByteWrapper {
    public byte Field { get; }
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class ByteWrapper {
    public void Read(IEndianBinaryReader er) {
      er.AssertByte(this.Field);
    }
  }
}
");
    }

    [Test]
    public void TestEverything() {
      this.AssertGenerated_(@"
using schema.binary;
using schema.binary.attributes.sequence;

namespace foo {
  namespace bar {
    [BinarySchema]
    public partial class EverythingWrapper : IBinaryConvertible {
      public readonly string magicText = ""foobar"";

      public byte byteField;
      public sbyte sbyteProperty { get; set; }
      public readonly short constShortField;
      public ushort constUshortProperty { get; }
     
      public ShortEnum nakedShortField;
      public readonly ShortEnum constNakedShortField;
      [IntegerFormat(SchemaIntegerType.INT32)]
      public ShortEnum intField;
      [IntegerFormat(SchemaIntegerType.INT32)]
      public readonly ShortEnum constIntField;

      public readonly int[] constLengthIntValues;
      [SequenceLengthSource(SchemaIntegerType.UINT32)]
      public int[] intValues;

      public Other other;
      [SequenceLengthSource(SchemaIntegerType.INT32)]
      public Other[] others;

      [NumberFormat(SchemaNumberType.UN16)]
      public float normalized;
      [NumberFormat(SchemaNumberType.UN16)]
      public readonly float constNormalized = 0;
    }

    public enum ShortEnum : short {
      A, B, C
    }

    [BinarySchema]
    public partial class Other : IBinaryConvertible {
    }
  }
}",
                            @"using System;
using System.Collections.Generic;
using System.IO;
namespace foo.bar {
  public partial class EverythingWrapper {
    public void Read(IEndianBinaryReader er) {" +
                            @"
      er.AssertString(this.magicText);" +
                            @"
      this.byteField = er.ReadByte();
      this.sbyteProperty = er.ReadSByte();
      er.AssertInt16(this.constShortField);
      er.AssertUInt16(this.constUshortProperty);" +
                            @"
      this.nakedShortField = (ShortEnum) er.ReadInt16();
      er.AssertInt16((short) this.constNakedShortField);
      this.intField = (ShortEnum) er.ReadInt32();
      er.AssertInt32((int) this.constIntField);" +
                            @"
      er.ReadInt32s(this.constLengthIntValues);
      {
        var c = er.ReadUInt32();
        if (c < 0) {
          throw new Exception(""Expected length to be nonnegative!"");
        }
        this.intValues = new int[c];
      }
      er.ReadInt32s(this.intValues);" +
                            @"
      this.other.Read(er);
      {
        var c = er.ReadInt32();
        if (c < 0) {
          throw new Exception(""Expected length to be nonnegative!"");
        }
        this.others = new Other[c];
        for (var i = 0; i < c; ++i) {
          this.others[i] = new Other();
        }
      }
      foreach (var e in this.others) {
        e.Read(er);
      }
      this.normalized = er.ReadUn16();
      er.AssertUn16(this.constNormalized);
    }
  }
}
");
    }

    private void AssertGenerated_(string src, string expectedGenerated) {
      var structure = BinarySchemaTestUtil.ParseFirst(src);
      Assert.IsEmpty(structure.Diagnostics);

      var actualGenerated = new BinarySchemaReaderGenerator().Generate(structure);
      Assert.AreEqual(expectedGenerated, actualGenerated.ReplaceLineEndings());
    }
  }
}