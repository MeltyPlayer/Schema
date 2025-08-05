using System;
using System.Collections.Generic;
using System.Linq;

using schema.util.strings;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public byte[] ReadBytes(char separator,
                          char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertByte_);

  public byte[] ReadHexBytes(char separator,
                             char terminator)
    => this.ConvertSplitUpToAndPastHexTerminator_(separator,
                                                  terminator,
                                                  this.ConvertHexByte_);


  public sbyte[] ReadSBytes(char separator,
                            char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertSByte_);

  public sbyte[] ReadHexSBytes(char separator,
                               char terminator)
    => this.ConvertSplitUpToAndPastHexTerminator_(separator,
                                                  terminator,
                                                  this.ConvertHexSByte_);


  public short[] ReadInt16s(char separator,
                            char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertInt16_);

  public short[] ReadHexInt16s(char separator,
                               char terminator)
    => this.ConvertSplitUpToAndPastHexTerminator_(separator,
                                                  terminator,
                                                  this.ConvertHexInt16_);


  public ushort[] ReadUInt16s(char separator,
                              char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertUInt16_);

  public ushort[] ReadHexUInt16s(char separator,
                                 char terminator)
    => this.ConvertSplitUpToAndPastHexTerminator_(separator,
                                                  terminator,
                                                  this.ConvertHexUInt16_);


  public int[] ReadInt32s(char separator,
                          char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertInt32_);


  public int[] ReadHexInt32s(char separator,
                             char terminator)
    => this.ConvertSplitUpToAndPastHexTerminator_(separator,
                                                  terminator,
                                                  this.ConvertHexInt32_);

  public uint[] ReadUInt32s(char separator,
                            char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertUInt32_);

  public uint[] ReadHexUInt32s(char separator,
                               char terminator)
    => this.ConvertSplitUpToAndPastHexTerminator_(separator,
                                                  terminator,
                                                  this.ConvertHexUInt32_);

  public long[] ReadInt64s(char separator,
                           char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertInt64_);

  public long[] ReadHexInt64s(char separator,
                              char terminator)
    => this.ConvertSplitUpToAndPastHexTerminator_(separator,
                                                  terminator,
                                                  this.ConvertHexInt64_);


  public ulong[] ReadUInt64s(char separator,
                             char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertUInt64_);


  public ulong[] ReadHexUInt64s(char separator,
                                char terminator)
    => this.ConvertSplitUpToAndPastHexTerminator_(separator,
                                                  terminator,
                                                  this.ConvertHexUInt64_);


  public float[] ReadSingles(char separator,
                             char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertSingle_);

  public double[] ReadDoubles(char separator,
                              char terminator)
    => this.ConvertSplitUpToAndPastTerminator_(separator,
                                               terminator,
                                               this.ConvertDouble_);


  private IEnumerable<string> ReadSplitUpToAndPastTerminator_(
      char separator,
      char terminator) {
    var match = this.ReadUpToAndPastTerminator(terminator);
    if (match.Length == 0) {
      return [];
    }

    return match.SplitViaChar(separator, false);
  }

  private T[] ConvertSplitUpToAndPastTerminator_<T>(
      char separator,
      char terminator,
      Func<string, T> converter)
    => this.ReadSplitUpToAndPastTerminator_(separator, terminator)
           .Select(t => {
                     var start = 0;

                     int i;
                     for (i = 0; i < t.Length; ++i) {
                       var c = t[i];
                       if (c is '\t' or ' ' or '\r' or '\n') {
                         start++;
                       }
                     }

                     if (t.Length - start == 0) {
                       return null;
                     }

                     return start == 0 ? t : t.Substring(start);
                   })
           .Where(text => text != null)
           .Select(converter)
           .ToArray();

  private T[] ConvertSplitUpToAndPastHexTerminator_<T>(
      char separator,
      char terminator,
      Func<string, T> converter)
    => this.ReadSplitUpToAndPastTerminator_(separator, terminator)
           .Select(t => {
                     var start = 0;

                     int i;
                     for (i = 0; i < t.Length; ++i) {
                       var c = t[i];
                       if (c is '\t' or ' ' or '\r' or '\n') {
                         start++;
                       } else {
                         break;
                       }
                     }

                     if (t.Length - start == 0) {
                       return null;
                     }

                     if (t[i] == '0' && i < t.Length - 1 && t[i + 1] == 'x') {
                       start += 2;
                     }

                     return start == 0 ? t : t.Substring(start);
                   })
           .Where(text => text != null)
           .Select(converter)
           .ToArray();
}