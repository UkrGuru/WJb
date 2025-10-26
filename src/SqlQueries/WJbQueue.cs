// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace UkrGuru.WJb.SqlQueries;

public static class WJbQueue
{
    public static readonly string Ins_Cron = """
        DECLARE @Now smalldatetime = GETDATE();
        INSERT INTO WJbQueue (RuleId, JobPriority, JobStatus)
        SELECT RuleId, RulePriority, 1 JobStatus    -- Queued
        FROM WJbRules
        WHERE Disabled = 0 
        AND dbo.CronValidate(JSON_VALUE(RuleMore, '$.cron'), @Now) = 1
        """;

    public static readonly string Start = """
        ;WITH cte AS (
            SELECT TOP (1) JobId, Started, JobStatus
            FROM WJbQueue
            WHERE Started IS NULL
            ORDER BY JobPriority ASC, JobId ASC
        )
        UPDATE cte
        SET Started = GETDATE(), JobStatus = 2      -- Running
        OUTPUT inserted.JobId;
        """;

    public static readonly string Finish = """
        DELETE FROM WJbQueue
        OUTPUT 
            deleted.JobId,
            deleted.JobPriority,
            deleted.Created,
            deleted.RuleId,
            deleted.Started,
            GETDATE() AS Finished,
            deleted.JobMore,
            @JobStatus AS JobStatus
        INTO WJbHistory
        WHERE JobId = @JobId;
        """;

    public static readonly string Finish_All = """
        DELETE FROM WJbQueue
        OUTPUT 
            deleted.JobId,
            deleted.JobPriority,
            deleted.Created,
            deleted.RuleId,
            deleted.Started,
            GETDATE() AS Finished,
            deleted.JobMore,
            5 JobStatus     -- Cancelled
        INTO WJbHistory
        WHERE Started IS NOT NULL;
        """;
    
    public static readonly string Get = """
        SELECT TOP (1) 
            Q.JobId, Q.JobPriority, Q.Created, Q.RuleId, Q.Started, Q.Finished, Q.JobMore, Q.JobStatus,
            R.RuleName, R.RuleMore, 
            A.ActionName, A.ActionType, A.ActionMore
        FROM WJbQueue Q
        INNER JOIN WJbRules R ON Q.RuleId = R.RuleId 
        INNER JOIN WJbActions A ON R.ActionId = A.ActionId
        WHERE Q.JobId = @Data
        """;
}
