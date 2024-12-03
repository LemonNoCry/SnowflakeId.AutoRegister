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

-- Print installation start message
SELECT 'Installing Snowflake SQL objects...';

-- Create the `Schema` table if it doesn't exist
CREATE TABLE IF NOT EXISTS `$(SnowflakeSchema)_Schema`
(
    `Version`
    INT
    NOT
    NULL,
    PRIMARY
    KEY
(
    `Version`
)
    ) ENGINE=InnoDB DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;

SELECT 'Created table `$(SnowflakeSchema)_Schema`';

-- Create `RegisterKeyValues` table
CREATE TABLE IF NOT EXISTS `$(SnowflakeSchema)_RegisterKeyValues`
(
    `Key`
    VARCHAR
(
    50
) NOT NULL,
    `Value` VARCHAR
(
    250
) NOT NULL,
    `CreatedTime` DATETIME NOT NULL DEFAULT NOW
(
),
    `ExpireTime` DATETIME NOT NULL,
    PRIMARY KEY
(
    `Key`
)
    ) ENGINE=InnoDB DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;

SELECT 'Created table `$(SnowflakeSchema)_RegisterKeyValues`';

-- Set the current schema version to 1
INSERT INTO `$(SnowflakeSchema)_Schema` (`Version`) VALUES (1) ON DUPLICATE KEY UPDATE `Version` = VALUES (`Version`);

SELECT 'Snowflake database schema installed';


