// global using 指令

global using SnowflakeId.AutoRegister.Configs;
global using SnowflakeId.AutoRegister.Core;
global using SnowflakeId.AutoRegister.Interfaces;
global using SnowflakeId.AutoRegister.Logging;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SnowflakeId.AutoRegister.Tests")]
[assembly: InternalsVisibleTo("SnowflakeId.AutoRegister.Tests.Redis")]
[assembly: InternalsVisibleTo("SnowflakeId.AutoRegister.Tests.MySql")]
[assembly: InternalsVisibleTo("SnowflakeId.AutoRegister.Tests.SqlServer")]