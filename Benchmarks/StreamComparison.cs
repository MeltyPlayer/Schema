using System.IO;

using BenchmarkDotNet.Attributes;

using schema.util.streams;


namespace benchmarks;

public class StreamComparison {
  public const int n = 10000;
  private readonly Stream stream_ = new MemoryStream(n);

  [IterationSetup]
  public void BeforeEach() {
    this.stream_.Position = 0;
  }

  [Benchmark]
  public void ReadByteThroughStream() {
    for (var i = 0; i < n; i++) {
      var b = this.stream_.ReadByte();
    }
  }

  [Benchmark]
  public void ReadByteThroughWrapper() {
    var reader = new ReadableStream(this.stream_);

    for (var i = 0; i < n; i++) {
      var b = reader.ReadByte();
    }
  }
}