BEGIN /***** Init Tables *****/

BEGIN /*** Init dbo.WJbActions ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WJbActions]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[WJbActions] (
		[ActionId] [int] IDENTITY(1,1) NOT NULL,
		[ActionName] [nvarchar](100) NOT NULL,
		[ActionType] [nvarchar](255) NOT NULL,
		[ActionMore] [nvarchar](2000) NULL,
		[Disabled] [bit] NOT NULL DEFAULT (0),
		CONSTRAINT [PK_WJbActions] PRIMARY KEY CLUSTERED ([ActionId] ASC),
		CONSTRAINT [UX_WJbActions_ActionName] UNIQUE NONCLUSTERED ([ActionName] ASC),
		CONSTRAINT [CK_WJbActions_ActionMore_ValidJson] CHECK ([ActionMore] IS NULL  OR isjson([ActionMore]) = (1) OR TRY_CAST([ActionMore] AS UNIQUEIDENTIFIER) IS NOT NULL)
	) ON [PRIMARY];

    --CREATE NONCLUSTERED INDEX IX_WJbActions_EnabledOnly ON dbo.WJbActions(ActionName) WHERE Disabled = 0;
END
END

BEGIN /*** Init WJbRules ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WJbRules]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[WJbRules] (
		[RuleId] [int] IDENTITY(1,1) NOT NULL,
		[RuleName] [nvarchar](100) NOT NULL,
		[Disabled] [bit] NOT NULL DEFAULT (0),
		[ActionId] [int] NOT NULL,
		[RulePriority] [tinyint] NOT NULL DEFAULT (2),
		[RuleMore] [nvarchar](2000) NULL,
	 CONSTRAINT [PK_WJbRules] PRIMARY KEY CLUSTERED ([RuleId] ASC),
	 CONSTRAINT [UX_WJbRules_RuleName] UNIQUE NONCLUSTERED ([RuleName] ASC),
	 CONSTRAINT [FK_WJbRules_WJbActions] FOREIGN KEY([ActionId]) REFERENCES [dbo].[WJbActions] ([ActionId]),
	 CONSTRAINT [CK_WJbRules_RuleMore_ValidJson] CHECK ([RuleMore] IS NULL OR isjson([RuleMore]) = (1) OR TRY_CAST([RuleMore] AS UNIQUEIDENTIFIER) IS NOT NULL)
	) ON [PRIMARY];
	
    CREATE NONCLUSTERED INDEX IX_WJbRules_ActionId ON dbo.WJbRules(ActionId);

    --CREATE NONCLUSTERED INDEX IX_WJbRules_RulePriority ON dbo.WJbRules(RulePriority);
END
END

BEGIN /*** Init WJbQueue ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WJbQueue]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WJbQueue](
        [JobId] [int] IDENTITY(1,1) NOT NULL,
        [JobPriority] [tinyint] NOT NULL DEFAULT (2),
        [Created] [datetime] NOT NULL DEFAULT (GETDATE()),
        [RuleId] [int] NOT NULL,
        [Started] [datetime] NULL,
        [Finished] [datetime] NULL,
        [JobMore] [nvarchar](2000) NULL,
        [JobStatus] [tinyint] NOT NULL DEFAULT (0),
        CONSTRAINT [PK_WJbQueue] PRIMARY KEY CLUSTERED ([JobId] ASC),
        CONSTRAINT [FK_WJbQueue_WJbRules] FOREIGN KEY ([RuleId]) REFERENCES [dbo].[WJbRules] ([RuleId]),
        CONSTRAINT [CK_WJbQueue_JobStatus] CHECK ([JobStatus] IN (0, 1, 2, 3, 4, 5)),
        CONSTRAINT [CK_WJbQueue_JobMore_ValidJson] CHECK ([JobMore] IS NULL OR isjson([JobMore]) = (1) OR TRY_CAST([JobMore] AS UNIQUEIDENTIFIER) IS NOT NULL)
    ) ON [PRIMARY];
END
END

BEGIN /*** Init WJbHistory ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WJbHistory]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[WJbHistory](
		[JobId] [int] NOT NULL,
		[JobPriority] [tinyint] NOT NULL,
		[Created] [datetime] NOT NULL,
		[RuleId] [int] NOT NULL,
		[Started] [datetime] NULL,
		[Finished] [datetime] NULL,
		[JobMore] [nvarchar](2000) NULL,
		[JobStatus] [tinyint] NOT NULL,
		CONSTRAINT [PK_WJbHistory] PRIMARY KEY CLUSTERED ([JobId] DESC),
		CONSTRAINT [FK_WJbHistory_WJbRules] FOREIGN KEY ([RuleId]) REFERENCES [dbo].[WJbRules] ([RuleId])
	) ON [PRIMARY];

	CREATE NONCLUSTERED INDEX [IX_WJbHistory_RuleId] ON [dbo].[WJbHistory] ([RuleId] ASC);

	CREATE NONCLUSTERED INDEX [IX_WJbHistory_CreatedDesc] ON [dbo].[WJbHistory] ([Created] DESC);
END
END

BEGIN /*** Init WJbSettings ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WJbSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WJbSettings](
        [Name] [nvarchar](100) NOT NULL,
        [Value] [nvarchar](max) NULL,
        CONSTRAINT [PK_WJbSettings] PRIMARY KEY CLUSTERED ([Name] ASC)
    ) ON [PRIMARY];
END
END

END

BEGIN /***** Cron Funcs *****/

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [dbo].[CronValidate] (@Expression varchar(100), @Now datetime)
RETURNS bit
AS
BEGIN
    IF @Expression LIKE ''%[A-Za-z]%''
    BEGIN
        SET @Expression = UPPER(@Expression);    

		;WITH Map AS (
			SELECT *
			FROM (VALUES
				(''JAN'', ''1''), (''FEB'', ''2''),  (''MAR'', ''3''),  (''APR'', ''4''),
				(''MAY'', ''5''), (''JUN'', ''6''),  (''JUL'', ''7''),  (''AUG'', ''8''),
				(''SEP'', ''9''), (''OCT'', ''10''), (''NOV'', ''11''), (''DEC'', ''12''),
				(''SUN'', ''0''), (''MON'', ''1''),  (''TUE'', ''2''),  (''WED'', ''3''),
				(''THU'', ''4''), (''FRI'', ''5''),  (''SAT'', ''6'')
			) AS M(OldVal, NewVal)
		)
        SELECT @Expression = REPLACE(@Expression, OldVal, NewVal)
        FROM Map;
    END

    IF @Expression LIKE ''%[^0-9*,/ -]%'' RETURN 0

    IF dbo.CronValidateWord(dbo.CronWord(@Expression, '' '', 1), DATEPART(MINUTE, @Now), 0, 59) = 0 RETURN 0;

    IF dbo.CronValidateWord(dbo.CronWord(@Expression, '' '', 2), DATEPART(HOUR, @Now), 0, 23) = 0 RETURN 0;

    IF dbo.CronValidateWord(dbo.CronWord(@Expression, '' '', 3), DATEPART(DAY, @Now), 1, 31) = 0 RETURN 0;

    IF dbo.CronValidateWord(dbo.CronWord(@Expression, '' '', 4), DATEPART(MONTH, @Now), 1, 12) = 0 RETURN 0;

    IF dbo.CronValidateWord(dbo.CronWord(@Expression, '' '', 5), dbo.CronWeekDay(@Now), 0, 6) = 0 RETURN 0;

    RETURN 1
END
';

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [dbo].[CronValidateWord](@parts varchar(100), @value int, @min int, @max int)
RETURNS tinyint
AS
BEGIN
    IF @value IS NULL OR @min IS NULL OR @max IS NULL OR NOT @value BETWEEN @min AND @max RETURN 0  

    DECLARE @cmmaPos int = CHARINDEX('','', @parts), @part varchar(100) = NULL;
    WHILE @cmmaPos > 0 OR LEN(@parts) > 0
    BEGIN
        SET @part = IIF(@cmmaPos > 0, LEFT(@parts, @cmmaPos - 1), @parts);

        IF @part = ''*'' RETURN 1;

        DECLARE @step INT = NULL, @start INT = NULL, @end INT = NULL;

        -- @step calculation, all drop after slash in @part
        DECLARE @slashPos int = CHARINDEX(''/'', @part);
        IF @slashPos > 0 BEGIN
            SET @step = TRY_CAST(SUBSTRING(@part, @slashPos + 1, LEN(@part)) AS INT);
            SET @part = LEFT(@part, @slashPos - 1)
        END
        SET @step = IIF(@step > 1, @step, 1);

        -- @start and @end calculation
        DECLARE @dashPos int = CHARINDEX(''-'', @part)
        IF @dashPos > 0 OR @slashPos > 0 BEGIN
            SET @start = IIF(@dashPos > 0, TRY_CAST(LEFT(@part, @dashPos - 1) AS INT), TRY_CAST(@part AS INT));
            IF @start IS NULL OR NOT @start BETWEEN @min AND @max RETURN 0
            SET @start = IIF(@start > @min, @start, @min);

            SET @end = IIF(@dashPos > 0, TRY_CAST(SUBSTRING(@part, @dashPos + 1, LEN(@part)) AS INT), @max);
            IF @end IS NULL OR NOT @end BETWEEN @min AND @max RETURN 0
            SET @end = IIF(@end < @max, @end, @max);

            -- and final search
            DECLARE @i int = @start;
            WHILE @i <= @end
            BEGIN
                IF @i = @value RETURN 1;
                SET @i += @step;
            END
        END 
        ELSE IF TRY_CAST(@part AS INT) = @value 
            RETURN 1;

        SET @parts = IIF(@cmmaPos > 0, SUBSTRING(@parts, @cmmaPos + 1, LEN(@parts)), '''');
        SET @cmmaPos = CHARINDEX('','', @parts);
    END

    RETURN 0;
END
';

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [dbo].[CronWeekDay](@Now datetime)
RETURNS int
AS
BEGIN
    RETURN (DATEPART(weekday, @Now) + @@DATEFIRST + 6) % 7
END
';

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [dbo].[CronWord] (@Words VARCHAR(100), @Separator VARCHAR(1), @Index INT)
RETURNS VARCHAR(100)
AS
BEGIN
    DECLARE @Word VARCHAR(100);

    IF @Words IS NULL OR @Index < 1 OR @Index > 5 RETURN NULL;

    ;WITH Split AS (
        SELECT value, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
        FROM STRING_SPLIT(@Words, @Separator)
    )
    SELECT @Word = value FROM Split WHERE rn = @Index;

    RETURN @Word;
END
';

END