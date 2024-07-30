# Snowflake Id(雪花Id) 自动注册

[![Latest version](https://img.shields.io/nuget/v/SnowflakeId.AutoRegister.svg?label=nuget)](https://www.nuget.org/packages/SnowflakeId.AutoRegister) [![License LGPLv3](https://img.shields.io/badge/license-MIT-blue)](https://choosealicense.com/licenses/mit/) [![Coverity Scan](https://scan.coverity.com/projects/30455/badge.svg)](https://scan.coverity.com/projects/lemonnocry-snowflakeid-autoregister)

这是一个自动注册 SnowflakeId 的 WorkerId 的 C# 库，支持 SQL Server、Redis 等。  
它本身不提供 SnowflakeId 的生成功能，只帮助你自动注册 WorkerIdn。  
理论上，SnowflakeId AutoRegister 可以与任何使用 SnowflakeId 的库集成。

- [简体中文](README.md)
- [English](README.en.md)

## 入门指南

SnowflakeId AutoRegister 是一个库，提供了一种简单的方法在 SnowflakeId 中自动注册 WorkerId。  
它本身不生成 Snowflake Id，只帮助你自动注册 WorkerId。

### 核心

- .NETStandard 2.0
- 支持 Redis
- 支持 SQL Server
- 未来更新将支持更多存储机制

### 安装

SnowflakeId.AutoRegister 作为 NuGet 包提供。你可以使用 NuGet 包管理控制台安装它：

#### 安装核心包

```bash
Install-Package SnowflakeId.AutoRegister
```

#### 使用 Redis

```bash
Install-Package SnowflakeId.AutoRegister.Redis
```

#### 使用 SqlServer

```bash
Install-Package SnowflakeId.AutoRegister.SqlServer
```

### 使用方法

使用 AutoRegisterBuilder 构建 IAutoRegister 的单例实例。

```csharp
static readonly IAutoRegister AutoRegister = new AutoRegisterBuilder()
    // 注册选项
    // 使用以下行设置标识符。
    // 推荐设置以区分单台机器上的多个应用程序
    .SetExtraIdentifier(Environment.CurrentDirectory)
    // 使用以下行设置 WorkerId 范围。
    .SetWorkerIdScope(1, 31)
    // 使用以下行设置注册选项。
    // .SetRegisterOption(option => {})

    // 使用以下行使用默认存储。
    // 仅适用于独立使用、本地测试等。
    //.UseDefaultStore()
        
    // 使用以下行使用 Redis 存储。
    .UseRedisStore("localhost:6379,allowAdmin=true")
       
    // 使用以下行使用 SQL Server 存储。
    //.UseSqlServerStore("Server=localhost;Database=IdGenerator;User Id=sa;Password=123456;")
    .Build();
```

使用 `AutoRegister` 实例获取 `SnowflakeIdConfig`。

```csharp
// 注册 WorkerId。
SnowflakeIdConfig config = AutoRegister.Register();
Console.WriteLine($"WorkerId: {config.WorkerId}");
```

### Yitter.IdGenerator 自动注册

```csharp
var config = AutoRegister.Register();
var options = new IdGeneratorOptions
{
    WorkerId = (ushort)config.WorkerId,
};
IIdGenerator idGenInstance = new DefaultIdGenerator(options);
long id = idGenInstance.NewLong();
Console.WriteLine($"Id: {id}");
```

### 其他自动注册

对于其他 Snowflake ID 生成库，请参考 Yitter.IdGenerator 示例。

## 构建源码

克隆仓库：

```bash
git clone https://github.com/LemonNoCry/SnowflakeId.AutoRegister.git
```

导航到项目目录：

```bash
cd SnowflakeId.AutoRegister 
```

恢复包：

```bash 
dotnet restore 
```

构建项目：

```bash
dotnet build
```

## 贡献

欢迎提交拉取请求。如果有重大更改，请先打开一个 Issue 讨论你想要更改的内容。
请确保适当更新测试。

## 许可证

[MIT](https://choosealicense.com/licenses/mit/)




