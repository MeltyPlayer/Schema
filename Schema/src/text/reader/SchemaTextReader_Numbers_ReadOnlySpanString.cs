using System;
using System.Collections.Generic;
using System.Linq;

using schema.util.strings;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public byte[] ReadBytes(ReadOnlySpan<string> separators,
                          ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertByte_);

  public byte[] ReadHexBytes(ReadOnlySpan<string> separators,
                             ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToHexTerminators_(separators,
                                            terminators,
                                            this.ConvertHexByte_);


  public sbyte[] ReadSBytes(ReadOnlySpan<string> separators,
                            ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertSByte_);

  public sbyte[] ReadHexSBytes(ReadOnlySpan<string> separators,
                               ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToHexTerminators_(separators,
                                            terminators,
                                            this.ConvertHexSByte_);


  public short[] ReadInt16s(ReadOnlySpan<string> separators,
                            ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertInt16_);

  public short[] ReadHexInt16s(ReadOnlySpan<string> separators,
                               ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToHexTerminators_(separators,
                                            terminators,
                                            this.ConvertHexInt16_);


  public ushort[] ReadUInt16s(ReadOnlySpan<string> separators,
                              ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertUInt16_);

  public ushort[] ReadHexUInt16s(ReadOnlySpan<string> separators,
                                 ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToHexTerminators_(separators,
                                            terminators,
                                            this.ConvertHexUInt16_);


  public int[] ReadInt32s(ReadOnlySpan<string> separators,
                          ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertInt32_);


  public int[] ReadHexInt32s(ReadOnlySpan<string> separators,
                             ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToHexTerminators_(separators,
                                            terminators,
                                            this.ConvertHexInt32_);

  public uint[] ReadUInt32s(ReadOnlySpan<string> separators,
                            ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertUInt32_);

  public uint[] ReadHexUInt32s(ReadOnlySpan<string> separators,
                               ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToHexTerminators_(separators,
                                            terminators,
                                            this.ConvertHexUInt32_);

  public long[] ReadInt64s(ReadOnlySpan<string> separators,
                           ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertInt64_);

  public long[] ReadHexInt64s(ReadOnlySpan<string> separators,
                              ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToHexTerminators_(separators,
                                            terminators,
                                            this.ConvertHexInt64_);


  public ulong[] ReadUInt64s(ReadOnlySpan<string> separators,
                             ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertUInt64_);


  public ulong[] ReadHexUInt64s(ReadOnlySpan<string> separators,
                                ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToHexTerminators_(separators,
                                            terminators,
                                            this.ConvertHexUInt64_);


  public float[] ReadSingles(ReadOnlySpan<string> separators,
                             ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertSingle_);

  public double[] ReadDoubles(ReadOnlySpan<string> separators,
                              ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToTerminators_(separators,
                                         terminators,
                                         this.ConvertDouble_);


  private IEnumerable<string> ReadSplitUpToTerminators_(
      ReadOnlySpan<string> separators,
      ReadOnlySpan<string> terminators) {
    var match = this.ReadUpToStartOfTerminator(terminators);
    if (match.Length == 0) {
      return [];
    }

    return match.SplitViaString(separators, false);
  }

  private T[] ConvertSplitUpToTerminators_<T>(
      ReadOnlySpan<string> separators,
      ReadOnlySpan<string> terminators,
      Func<string, T> converter)
    => this.ReadSplitUpToTerminators_(separators, terminators)
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

  private T[] ConvertSplitUpToHexTerminators_<T>(
      ReadOnlySpan<string> separators,
      ReadOnlySpan<string> terminators,
      Func<string, T> converter)
    => this.ReadSplitUpToTerminators_(separators, terminators)
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