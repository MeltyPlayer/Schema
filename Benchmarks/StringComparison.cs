using System;
using System.IO;

using BenchmarkDotNet.Attributes;


namespace benchmarks;

public class StringComparison {
  public const int w = 100;
  public const int n = 100;
  private readonly Stream stream_ = new MemoryStream(n);

  [IterationSetup]
  public void BeforeEach() {
    this.stream_.Position = 0;
  }

  [Benchmark]
  public void ReadCharsSeparately() {
    for (var iteration = 0; iteration < w; ++iteration) {
      this.stream_.Position = 0;

      Span<char> c = stackalloc char[n];
      for (var i = 0; i < n; i++) {
        c[i] = (char) this.stream_.ReadByte();
      }
    }
  }

  [Benchmark]
  public void ReadCharsViaByteBuffer() {
    for (var iteration = 0; iteration < w; ++iteration) {
      this.stream_.Position = 0;

      Span<byte> buffer = stackalloc byte[n];
      this.stream_.Read(buffer);

      Span<char> c = stackalloc char[n];
      for (var i = 0; i < n; i++) {
        c[i] = (char) buffer[i];
      }
    }
  }
}