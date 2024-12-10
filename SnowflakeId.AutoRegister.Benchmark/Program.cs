// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using SnowflakeId.AutoRegister.Benchmark.Core;

Console.WriteLine("Running Snowflake ID Benchmark with BenchmarkDotNet...");

BenchmarkRunner.Run<SnowflakeBenchmark>();

Console.WriteLine("Benchmarking complete.");
Console.ReadKey();