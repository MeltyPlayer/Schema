using System;

using schema.binary;
using schema.binary.attributes;

namespace build {
  [BinarySchema]
  public partial class ClassWithInt16Bools : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool Bool { get; private set; }

    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool ReadonlyBool { get; }

    [SequenceLengthSource(4)]
    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool[] Bools { get; set; }

    [SequenceLengthSource(4)]
    [IntegerFormat(SchemaIntegerType.INT16)]
    public bool[] ReadonlyBools { get; set; }
  }

  [BinarySchema]
  public partial class ClassWithInt32Bools : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.INT32)]
    public bool Bool { get; private set; }

    [IntegerFormat(SchemaIntegerType.INT32)]
    public bool ReadonlyBool { get; }

    [SequenceLengthSource(4)]
    [IntegerFormat(SchemaIntegerType.INT32)]
    public bool[] Bools { get; set; }

    [SequenceLengthSource(4)]
    [IntegerFormat(SchemaIntegerType.INT32)]
    public bool[] ReadonlyBools { get; set; }
  }

  [BinarySchema]
  public partial class ClassWithInt64Bools : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.INT64)]
    public bool Bool { get; private set; }

    [IntegerFormat(SchemaIntegerType.INT64)]
    public bool ReadonlyBool { get; }

    [SequenceLengthSource(4)]
    [IntegerFormat(SchemaIntegerType.INT64)]
    public bool[] Bools { get; set; }

    [SequenceLengthSource(4)]
    [IntegerFormat(SchemaIntegerType.INT64)]
    public bool[] ReadonlyBools { get; set; }
  }


  [BinarySchema]
  public partial class ClassWithChars : IBinaryConvertible {
    public char Char { get; private set; }

    public char ReadonlyChar { get; }

    [SequenceLengthSource(4)]
    public char[] Chars { get; set; }

    [SequenceLengthSource(4)]
    public char[] ReadonlyChars { get; set; }
  }


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

    public void Read(IEndianBinaryReader er) { }
  }
}