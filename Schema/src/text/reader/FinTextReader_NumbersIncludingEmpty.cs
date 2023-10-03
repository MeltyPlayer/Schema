using System;
using System.Collections.Generic;
using System.Linq;

namespace schema.text.reader {
  public sealed partial class FinTextReader {
    public byte?[] ReadBytesIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertByte_);

    public byte?[] ReadHexBytesIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
        terminators,
        this.ConvertHexByte_);


    public sbyte?[] ReadSBytesIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertSByte_);

    public sbyte?[] ReadHexSBytesIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
        terminators,
        this.ConvertHexSByte_);


    public short?[] ReadInt16sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertInt16_);

    public short?[] ReadHexInt16sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
        terminators,
        this.ConvertHexInt16_);


    public ushort?[] ReadUInt16sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertUInt16_);

    public ushort?[] ReadHexUInt16sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
        terminators,
        this.ConvertHexUInt16_);


    public int?[] ReadInt32sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertInt32_);


    public int?[] ReadHexInt32sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
        terminators,
        this.ConvertHexInt32_);

    public uint?[] ReadUInt32sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertUInt32_);

    public uint?[] ReadHexUInt32sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
        terminators,
        this.ConvertHexUInt32_);

    public long?[] ReadInt64sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertInt64_);

    public long?[] ReadHexInt64sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
        terminators,
        this.ConvertHexInt64_);


    public ulong?[] ReadUInt64sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertUInt64_);


    public ulong?[] ReadHexUInt64sIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminatorsIncludingEmpty_(separators,
        terminators,
        this.ConvertHexUInt64_);


    public float?[] ReadSinglesIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertSingle_);

    public double?[] ReadDoublesIncludingEmpty(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminatorsIncludingEmpty_(separators,
                                                  terminators,
                                                  this.ConvertDouble_);


    private IEnumerable<string> ReadSplitUpToAndPastTerminatorsIncludingEmpty_(
        string[] separators,
        string[] terminators) {
      var match = this.ReadUpToAndPastTerminator(terminators);
      if (match.Length == 0) {
        return Array.Empty<string>();
      }

      return match.Split(separators, StringSplitOptions.None);
    }

    private T?[] ConvertSplitUpToAndPastTerminatorsIncludingEmpty_<T>(
        string[] separators,
        string[] terminators,
        Func<string, T> converter) where T : struct
      => this.ReadSplitUpToAndPastTerminatorsIncludingEmpty_(separators, terminators)
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
        string[] separators,
        string[] terminators,
        Func<string, T> converter) where T : struct
      => this.ReadSplitUpToAndPastTerminatorsIncludingEmpty_(separators, terminators)
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
}