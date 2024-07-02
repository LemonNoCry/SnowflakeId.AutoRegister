# SnowflakeId AutoRegister

SnowflakeId AutoRegister is a C# project that provides an implementation of the SnowflakeId AutoRegister system, with
support for SQL Server, Redis, and potentially other storage mechanisms.

In theory, SnowflakeId AutoRegister can be integrated with any framework that utilizes SnowflakeId. This makes it a versatile solution for generating unique IDs across various platforms and technologies.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing
purposes.

### Prerequisites

- .NETStandard 2.0
- Support Redis
- Support SQL Server
- More storage mechanisms will be supported in future updates

### Installation

### Usage

This project can be used as a library in your .NET projects. It provides an easy way to generate unique IDs using the
Snowflake algorithm with support for SQL Server and Redis as storage mechanisms.

### Building the sources

Clone the repository:

```bash
git clone https://github.com/yourusername/SnowflakeId.AutoRegister.git
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

### Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
Please make sure to update tests as appropriate.

### License

[MIT](https://choosealicense.com/licenses/mit/)




