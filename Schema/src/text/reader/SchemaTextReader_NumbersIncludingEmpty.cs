using System;
using System.Collections.Generic;
using System.Linq;

using schema.util.strings;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public byte?[] ReadBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                         ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertByte_);

  public byte?[] ReadHexBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                            ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexByte_);


  public sbyte?[] ReadSBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                           ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertSByte_);

  public sbyte?[] ReadHexSBytesIncludingEmpty(ReadOnlySpan<string> separators,
                                              ReadOnlySpan<string>
                                                  terminators)
    => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexSByte_);


  public short?[] ReadInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                           ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertInt16_);

  public short?[] ReadHexInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                              ReadOnlySpan<string>
                                                  terminators)
    => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexInt16_);


  public ushort?[] ReadUInt16sIncludingEmpty(ReadOnlySpan<string> separators,
                                             ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertUInt16_);

  public ushort?[] ReadHexUInt16sIncludingEmpty(
      ReadOnlySpan<string> separators,
      ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexUInt16_);


  public int?[] ReadInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                         ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertInt32_);


  public int?[] ReadHexInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                            ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexInt32_);

  public uint?[] ReadUInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                           ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertUInt32_);

  public uint?[] ReadHexUInt32sIncludingEmpty(ReadOnlySpan<string> separators,
                                              ReadOnlySpan<string>
                                                  terminators)
    => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexUInt32_);

  public long?[] ReadInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                          ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertInt64_);

  public long?[] ReadHexInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                             ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexInt64_);


  public ulong?[] ReadUInt64sIncludingEmpty(ReadOnlySpan<string> separators,
                                            ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertUInt64_);


  public ulong?[] ReadHexUInt64sIncludingEmpty(
      ReadOnlySpan<string> separators,
      ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexUInt64_);


  public float?[] ReadSinglesIncludingEmpty(ReadOnlySpan<string> separators,
                                            ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertSingle_);

  public double?[] ReadDoublesIncludingEmpty(ReadOnlySpan<string> separators,
                                             ReadOnlySpan<string> terminators)
    => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertDouble_);


  private IEnumerable<string> ReadSplitUpToAndPastTerminatorsIncludingEmpty_(
      ReadOnlySpan<string> separators,
      ReadOnlySpan<string> terminators) {
    var match = this.ReadUpToAndPastTerminator(terminators);
    if (match.Length == 0) {
      return Array.Empty<string>();
    }

    return match.SplitViaString(separators, true);
  }

  private T?[] ConvertSplitUpToAndPastTerminatorsIncludingEmpty_<T>(
      ReadOnlySpan<string> separators,
      ReadOnlySpan<string> terminators,
      Func<string, T> converter) where T : struct
    => this.ReadSplitUpToAndPastTerminatorsIncludingEmpty_(
               separators,
               terminators)
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
           .Select(text => text != null ? converter(text) : (T?) null)
           .ToArray();

  private T?[] ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_<T>(
      ReadOnlySpan<string> separators,
      ReadOnlySpan<string> terminators,
      Func<string, T> converter) where T : struct
    => this.ReadSplitUpToAndPastTerminatorsIncludingEmpty_(
               separators,
               terminators)
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
                       return (T?) null;
                     }

                     if (t[i] == '0' && i < t.Length - 1 && t[i + 1] == 'x') {
                       start += 2;
                     }

                     return converter(start == 0 ? t : t.Substring(start));
                   })
           .ToArray();
}