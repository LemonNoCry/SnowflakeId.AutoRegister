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

For other Snowflake ID libraries, follow a similar approach to pass the WorkerId obtained from
`SnowflakeId.AutoRegister`.

---

## FAQ

- **What happens if the program crashes?**  
  WorkerId will not be released. On the next startup, the library attempts to reuse the previous WorkerId. If
  unsuccessful, a new WorkerId is assigned.

- **How to prevent duplicate WorkerId across processes?**  
  Use `SetExtraIdentifier` with process-specific data, such as the current process ID.

- **Is the default storage mechanism suitable for production?**  
  No, it is recommended only for development and testing. Use Redis, SQL Server, or MySQL for production environments.

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

## License

This project is licensed under the [MIT License](https://choosealicense.com/licenses/mit/).


