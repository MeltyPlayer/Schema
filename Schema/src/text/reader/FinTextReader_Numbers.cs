using System;
using System.Collections.Generic;
using System.Linq;

namespace schema.text.reader {
  public sealed partial class FinTextReader {
    public byte[] ReadBytes(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertByte_);

    public byte[] ReadHexBytes(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminators_(separators,
        terminators,
        this.ConvertHexByte_);


    public sbyte[] ReadSBytes(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertSByte_);

    public sbyte[] ReadHexSBytes(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminators_(separators,
        terminators,
        this.ConvertHexSByte_);


    public short[] ReadInt16s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertInt16_);

    public short[] ReadHexInt16s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminators_(separators,
        terminators,
        this.ConvertHexInt16_);


    public ushort[] ReadUInt16s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertUInt16_);

    public ushort[] ReadHexUInt16s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminators_(separators,
        terminators,
        this.ConvertHexUInt16_);


    public int[] ReadInt32s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertInt32_);


    public int[] ReadHexInt32s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminators_(separators,
        terminators,
        this.ConvertHexInt32_);

    public uint[] ReadUInt32s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertUInt32_);

    public uint[] ReadHexUInt32s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminators_(separators,
        terminators,
        this.ConvertHexUInt32_);

    public long[] ReadInt64s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertInt64_);

    public long[] ReadHexInt64s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminators_(separators,
        terminators,
        this.ConvertHexInt64_);


    public ulong[] ReadUInt64s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertUInt64_);


    public ulong[] ReadHexUInt64s(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastHexTerminators_(separators,
        terminators,
        this.ConvertHexUInt64_);


    public float[] ReadSingles(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertSingle_);

    public double[] ReadDoubles(string[] separators, string[] terminators)
      => this.ConvertSplitUpToAndPastTerminators_(separators,
                                                  terminators,
                                                  this.ConvertDouble_);


    private IEnumerable<string> ReadSplitUpToAndPastTerminators_(
        string[] separators,
        string[] terminators) {
      var match = this.ReadUpToAndPastTerminator(terminators);
      if (match.Length == 0) {
        return Enumerable.Empty<string>();
      }

      return match.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    private T[] ConvertSplitUpToAndPastTerminators_<T>(
        string[] separators,
        string[] terminators,
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
        string[] separators,
        string[] terminators,
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
}