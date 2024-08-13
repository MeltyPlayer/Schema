using System;
using System.IO;

using BenchmarkDotNet.Attributes;

using schema.binary;


namespace benchmarks;

[MemoryDiagnoser]
public class ReadingValues {
  public const int w = 1000;
  public const int m = 4;
  public const int n = 10000;

  [Params(Endianness.BigEndian, Endianness.LittleEndian)]
  public Endianness Endianness { get; set; }


  private readonly Stream stream_ = new MemoryStream(m * n);

  private SchemaBinaryReader BinaryReader { get; set; }

  [IterationSetup]
  public void BeforeEach() {
    this.BinaryReader
        = new SchemaBinaryReader(this.stream_, this.Endianness);
  }

  [Benchmark]
  public void ReadBytesSeparately() {
    for (var iteration = 0; iteration < w; iteration++) {
      this.stream_.Position = 0;
      for (var i = 0; i < n; ++i) {
        this.BinaryReader.ReadByte();
      }
    }
  }

  [Benchmark]
  public void ReadBytesTogether() {
    Span<byte> bytes = stackalloc byte[n];
    for (var iteration = 0; iteration < w; iteration++) {
      this.stream_.Position = 0;
      this.BinaryReader.ReadBytes(bytes);
    }
  }

  [Benchmark]
  public void ReadFloatsSeparately() {
    for (var iteration = 0; iteration < w; iteration++) {
      this.stream_.Position = 0;
      for (var i = 0; i < n; ++i) {
        this.BinaryReader.ReadSingle();
      }
    }
  }

  [Benchmark]
  public void ReadFloatsTogether() {
    Span<float> floats = stackalloc float[n];
    for (var iteration = 0; iteration < w; iteration++) {
      this.stream_.Position = 0;
      this.BinaryReader.ReadSingles(floats);
    }
  }
}