// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace UkrGuru.WJb.SqlQueries;

public static class WJbSettings
{
    public static readonly string Get = """
        SELECT TOP (1) [Value]
        FROM dbo.WJbSettings
        WHERE [Name] = @Data
        """;
}
