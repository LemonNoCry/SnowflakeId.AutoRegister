using System.Collections.Concurrent;
using System.Diagnostics;

namespace SnowflakeId.AutoRegister.Benchmark.Core;

public class SnowflakeTest(Func<long> genSnowflakeFunc)
{
    public static void Run()
    {
        // Initialize the SnowflakeId generator
        AutoRegisterIdGeneratorUtil.NextId();
        IdGeneratorUtil.NextId();

        {
            Console.WriteLine("IdGenerator Benchmark");
            var benchmark = new SnowflakeTest(IdGeneratorUtil.NextId);

            // Run single-threaded performance test
            benchmark.SingleThreadTest(1000000);

            // Run multi-threaded performance test
            benchmark.MultiThreadTest(10, 100000);

            // Run latency test
            benchmark.LatencyTest(100000);

            // Run distribution test
            benchmark.DistributionTest(100000);
            Console.WriteLine("=============================================================");
        }

        {
            Console.WriteLine("AutoRegister Benchmark");
            var benchmark = new SnowflakeTest(AutoRegisterIdGeneratorUtil.NextId);

            // Run single-threaded performance test
            benchmark.SingleThreadTest(1000000);

            // Run multi-threaded performance test
            benchmark.MultiThreadTest(10, 100000);

            // Run latency test
            benchmark.LatencyTest(100000);

            // Run distribution test
            benchmark.DistributionTest(100000);
            Console.WriteLine("=============================================================");
        }
    }

    /// <summary>
    /// Test single-threaded ID generation performance
    /// </summary>
    /// <param name="count">The number of IDs to generate</param>
    public void SingleThreadTest(int count = 0)
    {
        Console.WriteLine("Starting single-threaded performance test...");
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < count; i++) genSnowflakeFunc(); // Generate ID

        sw.Stop();

        Console.WriteLine($"Time taken to generate {count} IDs in single-threaded mode: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"IDs generated per second: {count / (sw.ElapsedMilliseconds / 1000.0):F2}");
    }

    /// <summary>
    /// Test multi-threaded ID generation performance
    /// </summary>
    /// <param name="threadCount">The number of threads to run</param>
    /// <param name="idsPerThread">The number of IDs generated per thread</param>
    public void MultiThreadTest(int threadCount = 0, int idsPerThread = 0)
    {
        Console.WriteLine("Starting multi-threaded performance test...");
        var allIds = new ConcurrentBag<long>();
        var sw = Stopwatch.StartNew();
        Parallel.For(0, threadCount, _ =>
        {
            for (var i = 0; i < idsPerThread; i++)
            {
                var id = genSnowflakeFunc();
                allIds.Add(id);
            }
        });
        sw.Stop();

        Console.WriteLine($"Time taken to generate {threadCount * idsPerThread} IDs in multi-threaded mode: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"IDs generated per second: {threadCount * idsPerThread / (sw.ElapsedMilliseconds / 1000.0):F2}");

        // Check for uniqueness
        Console.WriteLine($"Are all IDs unique: {allIds.Count == allIds.Distinct().Count()}");
    }

    /// <summary>
    /// Test the latency of generating a single ID
    /// </summary>
    /// <param name="count">The number of IDs to generate</param>
    public void LatencyTest(int count = 0)
    {
        Console.WriteLine("Starting latency test...");
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < count; i++) genSnowflakeFunc(); // Generate ID

        sw.Stop();

        Console.WriteLine($"Average time to generate a single ID: {sw.Elapsed.TotalMilliseconds / count:F6} ms");
    }

    /// <summary>
    /// Analyze the distribution of generated IDs
    /// </summary>
    /// <param name="count">The number of IDs to generate</param>
    public void DistributionTest(int count = 0)
    {
        Console.WriteLine("Starting distribution test...");
        var ids = new long[count];
        for (var i = 0; i < count; i++) ids[i] = genSnowflakeFunc();

        // Check high bits distribution
        var highBits = ids.Select(id => id >> 32).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        Console.WriteLine($"High bits distribution: {string.Join(", ", highBits.Select(kv => $"{kv.Key}: {kv.Value}"))}");
    }
}