// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace UkrGuru.WJb.SqlQueries;

public static class WJbQueue
{
    public static readonly string Ins_Cron = """
        ;WITH ValidRules AS (
            SELECT RuleId, RulePriority, JSON_VALUE(RuleMore, '$.cron') AS CronExpr
            FROM dbo.WJbRules
            WHERE Disabled = 0
        )
        INSERT INTO dbo.WJbQueue (RuleId, JobPriority, JobStatus)
        SELECT RuleId, RulePriority, 1  -- Queue
        FROM ValidRules
        WHERE dbo.CronValidate(CronExpr, GETDATE()) = 1
        """;

    public static readonly string Start1st = """
        ;WITH cte AS (
            SELECT TOP (1) JobId, Started, JobStatus
            FROM dbo.WJbQueue
            WHERE Started IS NULL
            ORDER BY JobPriority ASC, JobId ASC
        )
        UPDATE cte
        SET Started = GETDATE(), JobStatus = 2  -- Running
        OUTPUT inserted.JobId;
        """;

    public static readonly string Finish = """
        DELETE FROM dbo.WJbQueue
        OUTPUT 
            deleted.JobId,
            deleted.JobPriority,
            deleted.Created,
            deleted.RuleId,
            deleted.Started,
            GETDATE() AS Finished,
            deleted.JobMore,
            @JobStatus AS JobStatus
        INTO dbo.WJbHistory (
            JobId,
            JobPriority,
            Created,
            RuleId,
            Started,
            Finished,
            JobMore,
            JobStatus
        )
        WHERE JobId = @JobId;
        """;

    public static readonly string Finish_All = """
        DELETE FROM dbo.WJbQueue
        OUTPUT 
            deleted.JobId,
            deleted.JobPriority,
            deleted.Created,
            deleted.RuleId,
            deleted.Started,
            GETDATE() AS Finished,
            deleted.JobMore,
            5 JobStatus     -- Cancelled
        INTO dbo.WJbHistory (
            JobId,
            JobPriority,
            Created,
            RuleId,
            Started,
            Finished,
            JobMore,
            JobStatus
        )
        WHERE Started IS NOT NULL;
        """;
    
    public static readonly string Get = """
        SELECT TOP (1) 
            Q.JobId, Q.JobPriority, Q.Created, Q.RuleId, Q.Started, Q.Finished, Q.JobMore, Q.JobStatus,
            R.RuleName, R.RuleMore, 
            A.ActionName, A.ActionType, A.ActionMore
        FROM dbo.WJbQueue Q
        INNER JOIN dbo.WJbRules R ON Q.RuleId = R.RuleId 
        INNER JOIN dbo.WJbActions A ON R.ActionId = A.ActionId
        WHERE Q.JobId = @Data
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        """;
}
