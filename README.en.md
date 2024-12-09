# Snowflake Id AutoRegister

[![Latest version](https://img.shields.io/nuget/v/SnowflakeId.AutoRegister.svg?label=nuget)](https://www.nuget.org/packages/SnowflakeId.AutoRegister) [![License LGPLv3](https://img.shields.io/badge/license-MIT-blue)](https://choosealicense.com/licenses/mit/) [![Coverity Scan](https://scan.coverity.com/projects/30455/badge.svg)](https://scan.coverity.com/projects/lemonnocry-snowflakeid-autoregister)

`SnowflakeId.AutoRegister` is a lightweight C# library designed for automatic registration of WorkerId in SnowflakeId
systems. It supports various storage mechanisms, such as SQL Server, Redis, and MySQL.  
**Note:** This library does not generate Snowflake IDs itself. It is only responsible for WorkerId assignment and
registration, making it compatible with any framework or library using Snowflake IDs.

- [简体中文](README.md)
- [English](README.en.md)

---

## Key Features

- **Multi-storage support**: Compatible with Redis, SQL Server, MySQL, and more.
- **Lightweight design**: Avoids unnecessary dependencies; drivers are loaded dynamically at runtime.
- **Flexible configuration**: Chainable API to customize registration logic.
- **High compatibility**: Supports .NET Standard 2.0, allowing cross-platform usage.
- **Simplifies development**: Reduces complexity in managing WorkerId for distributed systems.
- **High reliability**: Supports automatic renewal of WorkerId to prevent duplicate assignments.

---

## Getting Started

`SnowflakeId.AutoRegister` simplifies the process of automatically registering WorkerIds in SnowflakeId systems.

### Prerequisites

- **.NET Standard 2.0** or later.
- Storage driver (e.g., Redis, SQL Server, MySQL).
- Suitable for applications requiring unique WorkerIds for SnowflakeId generation.

---

## Installation

Install the core package via NuGet:

```bash
Install-Package SnowflakeId.AutoRegister
```

### Optional Storage Support

#### Redis

```bash
Install-Package SnowflakeId.AutoRegister.Redis
```

#### SQL Server

```bash
Install-Package SnowflakeId.AutoRegister.SqlServer
```

**Note**: You must manually install the SQL Server driver if not already available:

```bash
Install-Package Microsoft.Data.SqlClient
```

or

```bash
Install-Package System.Data.SqlClient
```

#### MySQL

```bash
Install-Package SnowflakeId.AutoRegister.MySql
```

**Note**: You must manually install the MySQL driver if not already available:

```bash
Install-Package MySql.Data
```

or

```bash
Install-Package MySqlConnector
```

---

## Quick Start

### Initialize `AutoRegister`

Create a singleton `IAutoRegister` instance using `AutoRegisterBuilder`:

```csharp
static readonly IAutoRegister AutoRegister = new AutoRegisterBuilder()
    // Set unique identifiers to distinguish applications.
    .SetExtraIdentifier(Environment.CurrentDirectory)

    // Optionally, distinguish processes with the same path.
    // .SetExtraIdentifier(Environment.CurrentDirectory + Process.GetCurrentProcess().Id)

    // Set Logger.
    .SetLogMinimumLevel(SnowflakeId.AutoRegister.Logging.LogLevel.Debug)
    .SetLogger((level, message, ex) => Console.WriteLine($"[{DateTime.Now}] [{level}] {message} {ex}"))
    
    // Set the range for WorkerIds.
    .SetWorkerIdScope(1, 31)

    // Use a default storage mechanism (for development only).
    //.UseDefaultStore()

    // Use Redis as the storage.
    .UseRedisStore("localhost:6379,allowAdmin=true")

    // Use SQL Server as the storage.
    //.UseSqlServerStore("Server=localhost;Database=SnowflakeTest;User Id=sa;Password=123456;")

    // Use MySQL as the storage.
    //.UseMySqlStore("Server=localhost;Port=3306;Database=snowflaketest;Uid=test;Pwd=123456;SslMode=None;")
    .Build();
```

### Register WorkerId

Retrieve the WorkerId configuration using the `Register` method:

```csharp
SnowflakeIdConfig config = AutoRegister.Register();
Console.WriteLine($"WorkerId: {config.WorkerId}");
```

### Unregister WorkerId on Exit

Unregister WorkerId to release resources when the application exits:

```csharp
AutoRegister.UnRegister();

// Use AppDomain events for graceful shutdown.
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    AutoRegister.UnRegister();
    Console.WriteLine("Unregistered.");
};

// For .NET Core and later, use IHostApplicationLifetime events.
applicationLifetime.ApplicationStopping.Register(() =>
{
    AutoRegister.UnRegister();
    Console.WriteLine("Unregistered.");
});
```

---

## Integration with Snowflake ID Libraries

### Yitter.IdGenerator Example

Here's how to integrate `SnowflakeId.AutoRegister` with Yitter.IdGenerator:

```csharp
var config = AutoRegister.Register();
var options = new IdGeneratorOptions
{
    WorkerId = (ushort)config.WorkerId,
};
IIdGenerator idGenInstance = new DefaultIdGenerator(options);
long id = idGenInstance.NewLong();
Console.WriteLine($"Generated ID: {id}");
```

## AdvancedUsage

### Managing the Lifecycle of `Snowflake ID Tool Library`

Delegate the lifecycle of the Snowflake ID tool library to the `AutoRegister` instance to avoid the "zombie problem".    
**Principle: Process A registers WorkerId 1, but due to various reasons (such as a short lifecycle, network issues, etc.), it cannot renew in time. In other processes, this
WorkerId is considered invalid, and process B will register the same WorkerId 1. When process A recovers, it will detect that WorkerId 1 is already in use and will cancel the
registration, re-registering the next time it is acquired.**

Usage only requires adjusting `Build`.

Here is an example of using `Yitter.IdGenerator`:

```csharp
//IAutoRegister => IAutoRegister<xxx>
static readonly IAutoRegister<IIdGenerator> AutoRegister = new AutoRegisterBuilder()
    
    // Same as other configurations
    ...
    
    // The key point is here
    .Build<IIdGenerator>(config => new DefaultIdGenerator(new IdGeneratorOptions()
            {
                WorkerId = (ushort)config.WorkerId
            }));

    //Get Id
    // Ensure to use `GetIdGenerator()` to get the `IdGenerator` instance each time, do not cache it, as it may re-register
    long id =autoRegister.GetIdGenerator().NewLong();
    Console.WriteLine($"Id: {id}");
```

### For other Snowflake ID generation libraries, refer to the above examples for integration.

---

## FAQ

* Q: Why do we need to auto-register WorkerId?
* A: Snowflake ID requires WorkerId to generate unique IDs. Auto-registering WorkerId can reduce the complexity of manual maintenance.


* Q: Will WorkerId be released if the program crashes?
* A: No. WorkerId has a lifecycle. If the program exits abnormally, it will try to register the previous WorkerId on the next startup. If it fails, it will re-register a new
  WorkerId.


* Q: **What is the "zombie problem"?**
* A: **For example, process A registers a WorkerId, but due to various reasons (such as a short lifecycle, network issues, etc.), it cannot renew in time. In other processes, this
  WorkerId is considered invalid, and process B will register the same WorkerId. If process A recovers, both process A and process B will use the same WorkerId, causing ID
  duplication. See [Advanced Usage](#AdvancedUsage) for the solution.**


* Q: How to avoid multiple processes in the same file from being assigned the same WorkerId?
* A: Add a process-related identifier in SetExtraIdentifier, such as the current process ID.


* Q: Is the default storage mechanism suitable for production environments?
* A: The default storage mechanism is only suitable for development and local testing (to maintain consistency). In production environments, it is recommended to use Redis, SQL
  Server, MySQL, etc.

---

## Contributing

Contributions are welcome! To contribute:

1. Fork this repository and create a new branch.
2. Ensure your changes pass all tests and are synced with the main branch.
3. For major changes, open an issue to discuss your proposal first.
4. Submit a pull request with a clear description of your changes.

---

## Building the Source Code

Clone the repository:

```bash
git clone https://github.com/LemonNoCry/SnowflakeId.AutoRegister.git
```

Navigate to the project directory:

```bash
cd SnowflakeId.AutoRegister
```

Restore dependencies:

```bash
dotnet restore
```

Build the project:

```bash
dotnet build
```

---

## 💕 Donation (捐赠)

Alipay:  
<img src="https://github.com/LemonNoCry/SnowflakeId.AutoRegister/blog/master/resource/alipay.jpg" width="300" />

Wechat:  
<img src="https://github.com/LemonNoCry/SnowflakeId.AutoRegister/blog/master/resource/alipay.jpg" width="300" />

---

## License

This project is licensed under the [MIT License](https://choosealicense.com/licenses/mit/).


