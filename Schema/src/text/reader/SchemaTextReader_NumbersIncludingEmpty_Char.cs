using System;
using System.Collections.Generic;
using System.Linq;

using schema.util.strings;


namespace schema.text.reader;

public sealed partial class SchemaTextReader {
  public byte?[] ReadBytesIncludingEmpty(char separators,
                                         char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertByte_);

  public byte?[] ReadHexBytesIncludingEmpty(char separators,
                                            char terminators)
    => this.ConvertSplitUpToHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexByte_);


  public sbyte?[] ReadSBytesIncludingEmpty(char separators,
                                           char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertSByte_);

  public sbyte?[] ReadHexSBytesIncludingEmpty(char separators,
                                              char
                                                  terminators)
    => this.ConvertSplitUpToHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexSByte_);


  public short?[] ReadInt16sIncludingEmpty(char separators,
                                           char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertInt16_);

  public short?[] ReadHexInt16sIncludingEmpty(char separators,
                                              char
                                                  terminators)
    => this.ConvertSplitUpToHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexInt16_);


  public ushort?[] ReadUInt16sIncludingEmpty(char separators,
                                             char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertUInt16_);

  public ushort?[] ReadHexUInt16sIncludingEmpty(
      char separators,
      char terminators)
    => this.ConvertSplitUpToHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexUInt16_);


  public int?[] ReadInt32sIncludingEmpty(char separators,
                                         char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertInt32_);


  public int?[] ReadHexInt32sIncludingEmpty(char separators,
                                            char terminators)
    => this.ConvertSplitUpToHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexInt32_);

  public uint?[] ReadUInt32sIncludingEmpty(char separators,
                                           char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertUInt32_);

  public uint?[] ReadHexUInt32sIncludingEmpty(char separators,
                                              char
                                                  terminators)
    => this.ConvertSplitUpToHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexUInt32_);

  public long?[] ReadInt64sIncludingEmpty(char separators,
                                          char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertInt64_);

  public long?[] ReadHexInt64sIncludingEmpty(char separators,
                                             char terminators)
    => this.ConvertSplitUpToHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexInt64_);


  public ulong?[] ReadUInt64sIncludingEmpty(char separators,
                                            char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertUInt64_);


  public ulong?[] ReadHexUInt64sIncludingEmpty(
      char separators,
      char terminators)
    => this.ConvertSplitUpToHexTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertHexUInt64_);


  public float?[] ReadSinglesIncludingEmpty(char separators,
                                            char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertSingle_);

  public double?[] ReadDoublesIncludingEmpty(char separators,
                                             char terminators)
    => this.ConvertSplitUpToTerminatorsIncludingEmpty_(separators,
      terminators,
      this.ConvertDouble_);


  private IEnumerable<string> ReadSplitUpToTerminatorsIncludingEmpty_(
      char separators,
      char terminators) {
    var match = this.ReadUpToStartOfTerminator(terminators);
    if (match.Length == 0) {
      return [];
    }

    return match.SplitViaChar(separators, true);
  }

  private T?[] ConvertSplitUpToTerminatorsIncludingEmpty_<T>(
      char separators,
      char terminators,
      Func<string, T> converter) where T : struct
    => this.ReadSplitUpToTerminatorsIncludingEmpty_(
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

  private T?[] ConvertSplitUpToHexTerminatorsIncludingEmpty_<T>(
      char separators,
      char terminators,
      Func<string, T> converter) where T : struct
    => this.ReadSplitUpToTerminatorsIncludingEmpty_(
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