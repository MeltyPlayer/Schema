using System;
using System.Collections.Generic;
using System.Linq;

using schema.util.strings;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public byte[] ReadBytes(ReadOnlySpan<char> separators,
                          ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertByte_);

  public byte[] ReadHexBytes(ReadOnlySpan<char> separators,
                             ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastHexTerminators_(separators,
                                                   terminators,
                                                   this.ConvertHexByte_);


  public sbyte[] ReadSBytes(ReadOnlySpan<char> separators,
                            ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertSByte_);

  public sbyte[] ReadHexSBytes(ReadOnlySpan<char> separators,
                               ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastHexTerminators_(separators,
                                                   terminators,
                                                   this.ConvertHexSByte_);


  public short[] ReadInt16s(ReadOnlySpan<char> separators,
                            ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertInt16_);

  public short[] ReadHexInt16s(ReadOnlySpan<char> separators,
                               ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastHexTerminators_(separators,
                                                   terminators,
                                                   this.ConvertHexInt16_);


  public ushort[] ReadUInt16s(ReadOnlySpan<char> separators,
                              ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertUInt16_);

  public ushort[] ReadHexUInt16s(ReadOnlySpan<char> separators,
                                 ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastHexTerminators_(separators,
                                                   terminators,
                                                   this.ConvertHexUInt16_);


  public int[] ReadInt32s(ReadOnlySpan<char> separators,
                          ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertInt32_);


  public int[] ReadHexInt32s(ReadOnlySpan<char> separators,
                             ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastHexTerminators_(separators,
                                                   terminators,
                                                   this.ConvertHexInt32_);

  public uint[] ReadUInt32s(ReadOnlySpan<char> separators,
                            ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertUInt32_);

  public uint[] ReadHexUInt32s(ReadOnlySpan<char> separators,
                               ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastHexTerminators_(separators,
                                                   terminators,
                                                   this.ConvertHexUInt32_);

  public long[] ReadInt64s(ReadOnlySpan<char> separators,
                           ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertInt64_);

  public long[] ReadHexInt64s(ReadOnlySpan<char> separators,
                              ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastHexTerminators_(separators,
                                                   terminators,
                                                   this.ConvertHexInt64_);


  public ulong[] ReadUInt64s(ReadOnlySpan<char> separators,
                             ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertUInt64_);


  public ulong[] ReadHexUInt64s(ReadOnlySpan<char> separators,
                                ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastHexTerminators_(separators,
                                                   terminators,
                                                   this.ConvertHexUInt64_);


  public float[] ReadSingles(ReadOnlySpan<char> separators,
                             ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertSingle_);

  public double[] ReadDoubles(ReadOnlySpan<char> separators,
                              ReadOnlySpan<char> terminators)
    => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                terminators,
                                                this.ConvertDouble_);


  private IEnumerable<string> ReadSplitUpToAndPastTerminators_(
      ReadOnlySpan<char> separators,
      ReadOnlySpan<char> terminators) {
    var match = this.ReadUpToAndPastTerminator(terminators);
    if (match.Length == 0) {
      return [];
    }

    return match.SplitViaChar(separators, false);
  }

  private T[] ConvertSplitUpToAndPastTerminators_<T>(
      ReadOnlySpan<char> separators,
      ReadOnlySpan<char> terminators,
      Func<string, T> converter)
    => this.ReadSplitUpToAndPastTerminators_(separators, terminators)
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

  private T[] ConvertSplitUpToAndPastHexTerminators_<T>(
      ReadOnlySpan<char> separators,
      ReadOnlySpan<char> terminators,
      Func<string, T> converter)
    => this.ReadSplitUpToAndPastTerminators_(separators, terminators)
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