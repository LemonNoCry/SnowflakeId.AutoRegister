// global using 指令

global using System.Runtime.CompilerServices;
global using SnowflakeId.AutoRegister.Builder;
global using SnowflakeId.AutoRegister.Interfaces;
global using SnowflakeId.AutoRegister.StackExchangeRedis.Configs;
global using SnowflakeId.AutoRegister.StackExchangeRedis.Storage;
global using StackExchange.Redis;

[assembly: InternalsVisibleTo("SnowflakeId.AutoRegister.Tests.Redis")]