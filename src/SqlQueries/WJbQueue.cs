namespace UkrGuru.WJb.SqlQueries;

internal static class WJbQueue
{
    internal static readonly string Get = """
        SELECT TOP (1) Q.*, R.RuleName, R.RuleMore, A.ActionName, A.ActionType, A.ActionMore
        FROM WJbQueue Q
        INNER JOIN WJbRules R ON Q.RuleId = R.RuleId 
        INNER JOIN WJbActions A ON R.ActionId = A.ActionId
        WHERE Q.JobId = @Data
        """;

    internal static readonly string Start = """
        ;WITH cte AS (
            SELECT TOP 1 JobId
            FROM WJbQueue
            WHERE Started IS NULL
            ORDER BY JobPriority ASC, JobId ASC
        )
        UPDATE cte
        SET Started = GETDATE(), JobStatus = 2 -- Running
        OUTPUT inserted.JobId;
        """;

    internal static readonly string Finish = """
        DELETE FROM WJbQueue (JobId, JobPriority, Created, RuleId, Started, Finished, JobMore, JobStatus)        
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
        WHERE JobId = @JobId AND Started IS NOT NULL;
        """;
}
