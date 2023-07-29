using System.Globalization;

namespace System.IO {
  public sealed partial class FinTextReader {
    private byte ConvertByte_(string text) => byte.Parse(text);

    private byte ConvertHexByte_(string text)
      => byte.Parse(text, NumberStyles.HexNumber);


    private sbyte ConvertSByte_(string text) => sbyte.Parse(text);

    private sbyte ConvertHexSByte_(string text)
      => sbyte.Parse(text, NumberStyles.HexNumber);


    private short ConvertInt16_(string text) => short.Parse(text);

    private short ConvertHexInt16_(string text)
      => short.Parse(text, NumberStyles.HexNumber);


    private ushort ConvertUInt16_(string text) => ushort.Parse(text);

    private ushort ConvertHexUInt16_(string text)
      => ushort.Parse(text, NumberStyles.HexNumber);


    private int ConvertInt32_(string text) => int.Parse(text);

    private int ConvertHexInt32_(string text)
      => int.Parse(text, NumberStyles.HexNumber);


    private uint ConvertUInt32_(string text) => uint.Parse(text);

    private uint ConvertHexUInt32_(string text)
      => uint.Parse(text, NumberStyles.HexNumber);


    private long ConvertInt64_(string text) => long.Parse(text);

    private long ConvertHexInt64_(string text)
      => long.Parse(text, NumberStyles.HexNumber);


    private ulong ConvertUInt64_(string text) => ulong.Parse(text);

    private ulong ConvertHexUInt64_(string text)
      => ulong.Parse(text, NumberStyles.HexNumber);


    private float ConvertSingle_(string text) => float.Parse(text);
    private double ConvertDouble_(string text) => double.Parse(text);
  }
}