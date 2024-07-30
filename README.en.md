# Snowflake Id AutoRegister
[![Latest version](https://img.shields.io/nuget/v/SnowflakeId.AutoRegister.svg?label=nuget)](https://www.nuget.org/packages/SnowflakeId.AutoRegister) [![License LGPLv3](https://img.shields.io/badge/license-MIT-blue)](https://choosealicense.com/licenses/mit/) [![Coverity Scan](https://scan.coverity.com/projects/30455/badge.svg)](https://scan.coverity.com/projects/lemonnocry-snowflakeid-autoregister)

Is a C# library for automatic registration of WorkerId in SnowflakeId systems,
supporting SQL Server, Redis , and other.  
Itself does not provide the generation of Snowflake Id, it only helps you to automatically register WorkerId.  
In theory, SnowflakeId AutoRegister can be integrated with any framework that utilizes SnowflakeId

- [简体中文](README.md)
- [English](README.en.md)

## Getting Started

SnowflakeId AutoRegister is a library that provides a simple way to automatically register WorkerId in SnowflakeId
systems.  
Itself does not provide the generation of Snowflake Id, it only helps you to automatically register WorkerId.

### Prerequisites

- .NETStandard 2.0
- Support Redis
- Support SQL Server
- More storage mechanisms will be supported in future updates

### Installation

SnowflakeId.AutoRegister is available as a NuGet package. You can install it using the NuGet Package Console window:

Install the core package

```bash
Install-Package SnowflakeId.AutoRegister
```

Use Redis

```bash
Install-Package SnowflakeId.AutoRegister.Redis
```

Use SqlServer

```bash
Install-Package SnowflakeId.AutoRegister.SqlServer
```

### Usage

Use the `AutoRegisterBuilder` to create a singleton instance of `IAutoRegister`.

```csharp
static readonly IAutoRegister AutoRegister = new AutoRegisterBuilder()
    // Register Option
    // Use the following line to set the identifier.
    // Recommended setting to distinguish multiple applications on a single machine
   .SetExtraIdentifier(Environment.CurrentDirectory)
    // Use the following line to set the WorkerId scope.
   .SetWorkerIdScope(1, 31)
    // Use the following line to set the register option.
    // .SetRegisterOption(option => {})

    // Use the following line to use the default store.
    // Only suitable for standalone use, local testing, etc.
    //.UseDefaultStore()
        
    // Use the following line to use the Redis store.
    .UseRedisStore("localhost:6379,allowAdmin=true")
       
    // Use the following line to use the SQL Server store.
    //.UseSqlServerStore("Server=localhost;Database=IdGenerator;User Id=sa;Password=123456;")
    Build();
```

Use the `AutoRegister` instance to get the SnowflakeIdConfig.

```csharp
// Use Register WorkerId.
SnowflakeIdConfig config = AutoRegister.Register();
Console.WriteLine($"WorkerId: {config.WorkerId}");
```

Actively unload WorkerId when the program exits.

```csharp
// Actively unregister WorkerId, call this when the program exits
// If the program exits unexpectedly, it will automatically attempt to retrieve the previous WorkerId on the next startup.
// If retrieval fails, it will register a new one
AutoRegister.UnRegister();

// You can use the AppDomain.CurrentDomain.ProcessExit event
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    builder.UnRegister();
    Console.WriteLine("Unregistered.");
};

// For .NET Core and later versions, you can use the ApplicationStopping event
applicationLifetime.ApplicationStopping.Register(() =>
{
    builder.UnRegister();
    Console.WriteLine("Unregistered.");
});

```


#### Yitter.IdGenerator AutoRegister

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

### Other AutoRegister

For other Snowflake ID generation libraries, please refer to the Yitter.IdGenerator Demo.

## Building the sources

Clone the repository:

```bash
git clone https://github.com/LemonNoCry/SnowflakeId.AutoRegister.git
```

Navigate to the project directory:

```bash
cd SnowflakeId.AutoRegister 
```

Restore the packages:

```bash 
dotnet restore 
```

Build the project:

```bash
dotnet build
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
Please make sure to update tests as appropriate.

## License

[MIT](https://choosealicense.com/licenses/mit/)




