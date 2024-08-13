using System.Text;

using BenchmarkDotNet.Attributes;

using Cysharp.Text;


namespace benchmarks;

public class StringBuilderComparison {
  public const int w = 100;
  public const int n = 100;

  [Benchmark]
  public void Stack() {
      for (var iteration = 0; iteration < w; ++iteration) {
        var sb = new StringBuilder();

        for (var i = 0; i < n; i++) {
          sb.Append('0');
        }

        var str = sb.ToString();
      }
    }

  [Benchmark]
  public void Heap16() {
      for (var iteration = 0; iteration < w; ++iteration) {
        var sb = ZString.CreateStringBuilder();

        for (var i = 0; i < n; i++) {
          sb.Append('0');
        }

        var str = sb.ToString();
      }
    }

  [Benchmark]
  public void Heap8() {
      for (var iteration = 0; iteration < w; ++iteration) {
        var sb = ZString.CreateUtf8StringBuilder();

        for (var i = 0; i < n; i++) {
          sb.Append('0');
        }

        var str = sb.ToString();
      }
    }
}