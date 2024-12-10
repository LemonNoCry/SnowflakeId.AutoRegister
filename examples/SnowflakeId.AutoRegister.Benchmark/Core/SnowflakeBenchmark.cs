using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace SnowflakeId.AutoRegister.Benchmark.Core;

[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class SnowflakeBenchmark
{
    [GlobalSetup]
    public void SetUp()
    {
        AutoRegisterIdGeneratorUtil.NextId();
        IdGeneratorUtil.NextId();
    }

    [Benchmark]
    public void IdGeneratorUtil_100()
    {
        for (var i = 0; i < 100; i++) IdGeneratorUtil.NextId();
    }

    [Benchmark]
    public void AutoRegisterIdGeneratorUtil_100()
    {
        for (var i = 0; i < 100; i++) AutoRegisterIdGeneratorUtil.NextId();
    }
}