using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;


namespace benchmarks {
  public class Program {
    public static void Main(string[] args) {
      var summary = BenchmarkRunner.Run<ReadingValues>(
          ManualConfig
              .Create(DefaultConfig.Instance)
              .AddDiagnoser(
                  new MemoryDiagnoser(new MemoryDiagnoserConfig(true)))
              .WithOptions(ConfigOptions.DisableOptimizationsValidator));
    }
  }
}