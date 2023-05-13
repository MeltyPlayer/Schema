using Microsoft.CodeAnalysis;

using NUnit.Framework;

namespace schema.binary.text {
  public class SchemaWriterGeneratorTests {
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
using System.IO;
namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte(this.Field);
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
using System.IO;
namespace foo.bar {
  public partial class SByteWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteSByte(this.Field);
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
using System.IO;
namespace foo.bar {
  public partial class ShortWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt16(this.Field);
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
using System.IO;
namespace foo.bar {
  public partial class ArrayWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt32s(this.field);
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
using System.IO;
namespace foo.bar {
  public partial class ArrayWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt32(this.length);
      ew.WriteInt32s(this.field);
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
using System.IO;
namespace foo.bar {
  static internal partial class Parent {
    protected partial class Middle {
      private partial class Wrapper {
        public void Write(ISubEndianBinaryWriter ew) {
          ew.WriteInt32(this.length);
          ew.WriteInt32((int) this.value);
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
using System.IO;
namespace foo.bar {
  public partial class CharWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteChars(this.Array);
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
using System.IO;
namespace foo.bar {
  public partial class ShortWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteInt16(this.Field);
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
using System.IO;
namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte(this.field);
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
using System.IO;
namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte(this.field);
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
using System.IO;
namespace foo.bar {
  public partial class ByteWrapper {
    public void Write(ISubEndianBinaryWriter ew) {
      ew.WriteByte(this.Field);
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
using System.IO;
namespace foo.bar {
  public partial class EverythingWrapper {
    public void Write(ISubEndianBinaryWriter ew) {" +
                            @"
      ew.WriteString(this.magicText);" +
                            @"
      ew.WriteByte(this.byteField);
      ew.WriteSByte(this.sbyteProperty);
      ew.WriteInt16(this.constShortField);
      ew.WriteUInt16(this.constUshortProperty);" +
                            @"
      ew.WriteInt16((short) this.nakedShortField);
      ew.WriteInt16((short) this.constNakedShortField);
      ew.WriteInt32((int) this.intField);
      ew.WriteInt32((int) this.constIntField);" +
                            @"
      ew.WriteInt32s(this.constLengthIntValues);
      ew.WriteUInt32((uint) this.intValues.Length);
      ew.WriteInt32s(this.intValues);" +
                            @"
      this.other.Write(ew);
      ew.WriteInt32((int) this.others.Length);
      foreach (var e in this.others) {
        e.Write(ew);
      }
      ew.WriteUn16(this.normalized);
      ew.WriteUn16(this.constNormalized);
    }
  }
}
");
    }

    private void AssertGenerated_(string src, string expectedGenerated) {
      var structure = BinarySchemaTestUtil.ParseFirst(src);
      Assert.IsEmpty(structure.Diagnostics);

      var actualGenerated = new BinarySchemaWriterGenerator().Generate(structure);
      Assert.AreEqual(expectedGenerated, actualGenerated.ReplaceLineEndings());
    }
  }
}