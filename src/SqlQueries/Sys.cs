// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace UkrGuru.WJb.SqlQueries;

public static class Sys
{
    public static readonly string Get_Ver = """
        SELECT TOP 1 [value]
        FROM sys.extended_properties
        WHERE class = 0 AND class_desc = N'DATABASE' AND [name] = @Data
        """;

    public static readonly string Upd_Ver = """
        BEGIN TRY 
            EXEC sp_updateextendedproperty @name = @Name, @value = @Value;  
        END TRY 
        BEGIN CATCH
            EXEC sp_addextendedproperty @name = @Name, @value = @Value;  
        END CATCH
        """;
}
