// global using 指令

global using System.Data.Common;
global using SnowflakeId.AutoRegister.Builder;
global using SnowflakeId.AutoRegister.Db.Configs;
global using SnowflakeId.AutoRegister.Db.Extensions;
global using SnowflakeId.AutoRegister.Db.Storage;
global using SnowflakeId.AutoRegister.Interfaces;
global using SnowflakeId.AutoRegister.SqlServer.Configs;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SnowflakeId.AutoRegister.Tests.SqlServer")]