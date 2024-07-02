-- MIT License
--
-- Copyright (c) 2024 LemonNoCry
--
-- Permission is hereby granted, free of charge, to any person obtaining a copy
-- of this software and associated documentation files (the "Software"), to deal
-- in the Software without restriction, including without limitation the rights
-- to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
-- copies of the Software, and to permit persons to whom the Software is
-- furnished to do so, subject to the following conditions:
--
-- The above copyright notice and this permission notice shall be included in all
-- copies or substantial portions of the Software.
--
-- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
-- IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
-- FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
-- AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
-- LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
-- OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
-- SOFTWARE.

SET NOCOUNT ON
SET XACT_ABORT ON
DECLARE @TARGET_SCHEMA_VERSION INT;
SET @TARGET_SCHEMA_VERSION = 1;

PRINT 'Installing Snowflake SQL objects...';

BEGIN TRANSACTION;

-- Acquire exclusive lock to prevent deadlocks caused by schema creation / version update
DECLARE @SchemaLockResult INT;
EXEC @SchemaLockResult = sp_getapplock @Resource = '$(SnowflakeSchema):SchemaLock', @LockMode = 'Exclusive'

-- Create the database schema if it doesn't exists
IF NOT EXISTS ( SELECT [schema_id] FROM [sys].[schemas] WHERE [name] = '$(SnowflakeSchema)' )
    BEGIN
        EXEC (N'CREATE SCHEMA [$(SnowflakeSchema)]');
        PRINT 'Created database schema [$(SnowflakeSchema)]';
    END
ELSE
    PRINT 'Database schema [$(SnowflakeSchema)] already exists';

DECLARE @SCHEMA_ID INT;
SELECT @SCHEMA_ID = [schema_id] FROM [sys].[schemas] WHERE [name] = '$(SnowflakeSchema)';

-- Create the [$(SnowflakeSchema)].Schema table if not exists
IF NOT EXISTS ( SELECT [object_id] FROM [sys].[tables] WHERE [name] = 'Schema' AND [schema_id] = @SCHEMA_ID )
    BEGIN
        CREATE TABLE [$(SnowflakeSchema)].[Schema]
        (
            [Version] [int] NOT NULL,
            CONSTRAINT [PK_Snowflake_Schema] PRIMARY KEY CLUSTERED ([Version] ASC)
        );
        PRINT 'Created table [$(SnowflakeSchema)].[Schema]';
    END
ELSE
    PRINT 'Table [$(SnowflakeSchema)].[Schema] already exists';

DECLARE @CURRENT_SCHEMA_VERSION INT;
SELECT @CURRENT_SCHEMA_VERSION = [Version] FROM [$(SnowflakeSchema)].[Schema];

PRINT 'Current Snowflake schema version: ' +
      IIF(@CURRENT_SCHEMA_VERSION IS NULL, 'none', CONVERT(NVARCHAR, @CURRENT_SCHEMA_VERSION));

IF @CURRENT_SCHEMA_VERSION IS NOT NULL AND @CURRENT_SCHEMA_VERSION > @TARGET_SCHEMA_VERSION
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT 'Snowflake current database schema version ' + CAST(@CURRENT_SCHEMA_VERSION AS NVARCHAR) +
              ' is newer than the configured SqlServerStorage schema version ' +
              CAST(@TARGET_SCHEMA_VERSION AS NVARCHAR) + '. Will not apply any migrations.';
        RETURN;
    END

-- Install [$(SnowflakeSchema)] schema objects
IF @CURRENT_SCHEMA_VERSION IS NULL
    BEGIN
        PRINT 'Installing schema version 1';

        -- Create  RegisterKeyValues tables
        CREATE TABLE [$(SnowflakeSchema)].[RegisterKeyValues]
        (
            [Key]         [varchar](50)  NOT NULL,
            [Value]       [varchar](250) NOT NULL,
            [CreatedTime] [datetime]     NOT NULL DEFAULT (GETDATE()),
            [ExpireTime]  [datetime]     NOT NULL,
            CONSTRAINT [PK_Snowflake_SnowflakeId] PRIMARY KEY CLUSTERED ([Key] ASC)
        );

        PRINT 'Created table [$(SnowflakeSchema)].[RegisterKeyValues]';

        SET @CURRENT_SCHEMA_VERSION = 2;
    END

-- IF @CURRENT_SCHEMA_VERSION = 2
--     BEGIN
--         PRINT 'Installing schema version 2';
--     END

UPDATE [$(SnowflakeSchema)].[Schema] SET [Version] = @CURRENT_SCHEMA_VERSION
IF @@ROWCOUNT = 0 INSERT INTO [$(SnowflakeSchema)].[Schema] ([Version]) VALUES (@CURRENT_SCHEMA_VERSION)

PRINT 'Snowflake database schema installed';

COMMIT TRANSACTION;
PRINT 'Snowflake SQL objects installed';
