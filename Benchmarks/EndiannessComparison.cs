using System;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using CommunityToolkit.HighPerformance;

using schema.binary;


namespace benchmarks;

[MemoryDiagnoser]
public class EndiannessComparison {
  public const int n = 10000;

  [Params(Endianness.BigEndian, Endianness.LittleEndian)]
  public Endianness Endianness { get; set; }

  public uint[] values_ = new uint[n];

  [Benchmark]
  public void ReverseSpansManually() {
    var span = this.values_.AsSpan().AsBytes();
    for (var i = 0; i < span.Length; i++) {
      span.Slice(4 * i, 4).Reverse();
    }
  }

  [Benchmark]
  public void ReverseSpansUsingReverser() {
    var reverser = new SpanElementReverser();

    var span = this.values_.AsSpan().AsBytes();
    reverser.ReverseElements(span, 4);
  }

  [Benchmark]
  public void ReverseSpansUsingReverserInterface() {
    ISpanElementReverser reverser = new SpanElementReverser();

    var span = this.values_.AsSpan().AsBytes();
    reverser.ReverseElements(span, 4);
  }

  [Benchmark]
  public void ReverseSpansUsingReverserNoop() {
    var reverser = new NoopSpanElementReverser();

    var span = this.values_.AsSpan().AsBytes();
    reverser.ReverseElements(span, 4);
  }

  [Benchmark]
  public void ReverseSpansUsingReverserNoopInterface() {
    ISpanElementReverser reverser = new NoopSpanElementReverser();

    var span = this.values_.AsSpan().AsBytes();
    reverser.ReverseElements(span, 4);
  }

  [Benchmark]
  public void ReverseSpansAsUintsInlined() {
    var span = this.values_.AsSpan();
    for (var i = 0; i < span.Length; i++) {
      var value = span[i];
      span[i] = (value & 0x000000FFU) << 24 |
                (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 |
                (value & 0xFF000000U) >> 24;
    }
  }

  [Benchmark]
  public void ReverseSpansAsUintsViaMethod() {
    var span = this.values_.AsSpan();
    for (var i = 0; i < span.Length; i++) {
      span[i] = ReverseBytes(span[i]);
    }
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static uint ReverseBytes(uint value) {
    return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
           (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
  }
}