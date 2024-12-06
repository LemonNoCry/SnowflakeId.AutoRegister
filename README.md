# Snowflake Id(雪花Id) 自动注册

[![Latest version](https://img.shields.io/nuget/v/SnowflakeId.AutoRegister.svg?label=nuget)](https://www.nuget.org/packages/SnowflakeId.AutoRegister) [![License LGPLv3](https://img.shields.io/badge/license-MIT-blue)](https://choosealicense.com/licenses/mit/) [![Coverity Scan](https://scan.coverity.com/projects/30455/badge.svg)](https://scan.coverity.com/projects/lemonnocry-snowflakeid-autoregister)

`SnowflakeId.AutoRegister` 是一个 C# 库，帮助你为 Snowflake ID 自动注册 WorkerId。
它不生成 Snowflake ID，仅为 WorkerId 的分配和注册提供支持。
支持多种存储机制（SQL Server、Redis 等），可轻松集成到任何使用 Snowflake ID 的库中。

- [简体中文](README.md)
- [English](README.en.md)

---

## 入门指南

SnowflakeId AutoRegister 是一个库，提供了一种简单的方法在 SnowflakeId 中自动注册 WorkerId。  
它本身不生成 Snowflake Id，只帮助你自动注册 WorkerId。

## 核心特点

* 多存储机制支持：Redis、SQL Server、MySQL 等
* 轻量级设计：无依赖，运行时动态加载驱动
* 灵活配置：通过链式 API 自定义注册逻辑
* 高兼容性：支持 .NET Standard 2.0，可在多种平台运行
* 简化开发流程：减少手动维护 WorkerId 的复杂性

---

## 注意

* **为了兼容多种驱动以及多种版本,不包含任何驱动,避免过多依赖,运行时动态加载驱动**

## 安装

### 安装核心包

使用 NuGet 包管理工具快速安装：

```bash
Install-Package SnowflakeId.AutoRegister
```

### 可选存储支持

* #### Redis 存储支持：

  ```bash
  Install-Package SnowflakeId.AutoRegister.Redis
  ```

* #### SQL Server 存储支持：

    ```bash
    Install-Package SnowflakeId.AutoRegister.SqlServer
    ```
  **注意**：需自行安装 SQL Server 驱动：`Microsoft.Data.SqlClient`、`System.Data.SqlClient`  
  一般情况业务上都有安装对应驱动,如果没有安装,请自行安装

  ```bash
  Install-Package Microsoft.Data.SqlClient
  ```
  或
  ```bash
  Install-Package System.Data.SqlClient
  ```

* #### MySQL 存储支持：

  ```bash
  Install-Package SnowflakeId.AutoRegister.MySql
  ```

  **注意**：需自行安装 MySQL 驱动`MySql.Data`、`MySqlConnector`  
  一般情况业务上都有安装对应驱动,如果没有安装,请自行安装

  ```bash
  Install-Package MySql.Data
  ```
  或
* ```bash
  Install-Package MySqlConnector
  ```

---

## 快速开始

以下是使用 SnowflakeId.AutoRegister 的基本示例

### 初始化 `AutoRegister` 实例

使用 `AutoRegisterBuilder` 构建一个单例实例：

```csharp
static readonly IAutoRegister AutoRegister = new AutoRegisterBuilder()
    // 注册选项
    // 使用以下行设置标识符。
    // 推荐设置以区分单台机器上的多个应用程序
    .SetExtraIdentifier(Environment.CurrentDirectory)
    
    // 区分同路径exe,多个进程
    // .SetExtraIdentifier(Environment.CurrentDirectory + Process.GetCurrentProcess().Id)S
    
    // 日志配置
    .SetLogMinimumLevel(SnowflakeId.AutoRegister.Logging.LogLevel.Debug)
    .SetLogger((level, message, ex) => Console.WriteLine($"[{DateTime.Now}] [{level}] {message} {ex}"))
    
    // 使用以下行设置 WorkerId 范围。
    .SetWorkerIdScope(1, 31)
    // 使用以下行设置注册选项。
    // .SetRegisterOption(option => {})

    // 使用以下行使用默认存储。
    // 仅适用于开发使用、本地测试等。
    //.UseDefaultStore()
        
    // 使用以下行使用 Redis 存储。
    .UseRedisStore("localhost:6379,allowAdmin=true")
       
    // 使用以下行使用 SQL Server 存储。
    //.UseSqlServerStore("Server=localhost;Database=SnowflakeTest;User Id=sa;Password=123456;")
                
    // Use the following line to use the MySQL store.
    .UseMySqlStore("Server=localhost;Port=3306;Database=snowflaketest;Uid=test;Pwd=123456;SslMode=None;")
    
    .Build();
```

### 注册 WorkerId

通过 `AutoRegister` 实例获取 `WorkerId` 配置：

```csharp
// 注册 WorkerId。
SnowflakeIdConfig config = AutoRegister.Register();
Console.WriteLine($"WorkerId: {config.WorkerId}");
```

### 程序退出时注销 WorkerId

在程序退出时，主动注销 WorkerId，确保资源释放：

```csharp
//主动注销WorkId,程序退出时调用
//如果程序异常退出，下次启动时会自动尝试获取上次的WorkerId,如果获取失败会重新注册
AutoRegister.UnRegister();

//可以使用AppDomain.CurrentDomain.ProcessExit事件
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    builder.UnRegister();
    Console.WriteLine("Unregistered.");
};

//.Net Core及以上版本可以使用ApplicationStopping事件
applicationLifetime.ApplicationStopping.Register(() =>
{
    builder.UnRegister();
    Console.WriteLine("Unregistered.");
});

```

---

## 集成其他 Snowflake ID 库

### Yitter.IdGenerator

以下是集成 Yitter.IdGenerator 的示例：

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

### 对于其他 Snowflake ID 生成库，可以参考上述示例进行集成。

---

## 常见问题 (FAQ)

* Q: 如果程序崩溃了，WorkerId 会被释放吗？
* A: 不会。程序异常退出时，下次启动会尝试分配上一次的 WorkerId。如果失败，则重新注册新的 WorkerId。


* Q: 如何避免多进程重复分配 WorkerId？
* A: 在 SetExtraIdentifier 中添加进程相关的标识符，例如当前进程 ID。


* Q: 默认存储机制适合生产环境吗？
* A: 默认存储机制仅适合开发和本地测试。在生产环境中，建议使用 Redis、SQL Server 或 MySQL 存储。

---

## 贡献指南

欢迎提交拉取请求！在贡献代码前，请遵循以下步骤：

1. Fork 本仓库并创建新分支。
2. 确保代码通过所有测试，并保持与主分支同步。
3. 如果有重大更改，请先打开一个 Issue 讨论你想要更改的内容。 请确保适当更新测试。
4. 提交 PR 并描述所做的更改。

---

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

---

## 许可证

This project is licensed under the [MIT License](https://choosealicense.com/licenses/mit/).


