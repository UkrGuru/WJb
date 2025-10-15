BEGIN /***** Init Tables *****/

BEGIN /*** Init WJbActions ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[WJbActions]') AND type in (N'U'))
BEGIN
	CREATE TABLE [WJbActions] (
		[ActionId] [int] IDENTITY(1000,1) NOT NULL,
		[ActionName] [nvarchar](100) NOT NULL,
		[ActionType] [nvarchar](255) NOT NULL,
		[ActionMore] [nvarchar](MAX) NULL,
		[Disabled] [bit] NOT NULL DEFAULT (0),
		CONSTRAINT [PK_WJbActions] PRIMARY KEY CLUSTERED ([ActionId] ASC),
		CONSTRAINT [UX_WJbActions_ActionName] UNIQUE NONCLUSTERED ([ActionName] ASC),
		CONSTRAINT [CK_WJbActions_ActionMore_ValidJson] CHECK ([ActionMore] IS NULL OR isjson([ActionMore]) = (1))
	) ON [PRIMARY]

    --CREATE NONCLUSTERED INDEX IX_WJbActions_EnabledOnly ON WJbActions(ActionName) WHERE Disabled = 0;
END
END

BEGIN /*** Init WJbRules ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[WJbRules]') AND type in (N'U'))
BEGIN
	CREATE TABLE [WJbRules] (
		[RuleId] [int] IDENTITY(1000,1) NOT NULL,
		[RuleName] [nvarchar](100) NOT NULL,
		[Disabled] [bit] NOT NULL DEFAULT (0),
		[ActionId] [int] NOT NULL,
		[RulePriority] [tinyint] NOT NULL DEFAULT (2),
		[RuleMore] [nvarchar](MAX) NULL,
	 CONSTRAINT [PK_WJbRules] PRIMARY KEY CLUSTERED ([RuleId] ASC),
	 CONSTRAINT [UX_WJbRules_RuleName] UNIQUE NONCLUSTERED ([RuleName] ASC),
	 CONSTRAINT [FK_WJbRules_WJbActions] FOREIGN KEY([ActionId]) REFERENCES [dbo].[WJbActions] ([ActionId]),
	 CONSTRAINT [CK_WJbRules_RuleMore_ValidJson] CHECK ([RuleMore] IS NULL OR isjson([RuleMore]) = (1))
	) ON [PRIMARY]
	
    CREATE NONCLUSTERED INDEX IX_WJbRules_ActionId ON WJbRules(ActionId);

    --CREATE NONCLUSTERED INDEX IX_WJbRules_RulePriority ON WJbRules(RulePriority);
END
END

BEGIN /*** Init WJbQueue ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[WJbQueue]') AND type in (N'U'))
BEGIN
    CREATE TABLE [WJbQueue](
        [JobId] [int] IDENTITY(1,1) NOT NULL,
        [JobPriority] [tinyint] NOT NULL DEFAULT (2),
        [Created] [datetime] NOT NULL DEFAULT (GETDATE()),
        [RuleId] [int] NOT NULL,
        [Started] [datetime] NULL,
        [Finished] [datetime] NULL,
        [JobMore] [nvarchar](MAX) NULL,
        [JobStatus] [tinyint] NOT NULL DEFAULT (0),
        CONSTRAINT [PK_WJbQueue] PRIMARY KEY CLUSTERED ([JobId] ASC),
        CONSTRAINT [FK_WJbQueue_WJbRules] FOREIGN KEY ([RuleId]) REFERENCES [WJbRules] ([RuleId]),
        CONSTRAINT [CK_WJbQueue_JobStatus] CHECK ([JobStatus] IN (0, 1, 2, 3, 4, 5)),
        CONSTRAINT [CK_WJbQueue_JobMore_ValidJson] CHECK ([JobMore] IS NULL OR isjson([JobMore]) = (1))
    ) ON [PRIMARY]
END
END

BEGIN /*** Init WJbHistory ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[WJbHistory]') AND type in (N'U'))
BEGIN
	CREATE TABLE [WJbHistory](
		[JobId] [int] NOT NULL,
		[JobPriority] [tinyint] NOT NULL,
		[Created] [datetime] NOT NULL,
		[RuleId] [int] NOT NULL,
		[Started] [datetime] NULL,
		[Finished] [datetime] NULL,
		[JobMore] [nvarchar](MAX) NULL,
		[JobStatus] [tinyint] NOT NULL,
		CONSTRAINT [PK_WJbHistory] PRIMARY KEY CLUSTERED ([JobId] DESC),
		CONSTRAINT [FK_WJbHistory_WJbRules] FOREIGN KEY ([RuleId]) REFERENCES [WJbRules] ([RuleId]),
		CONSTRAINT [CK_WJbHistory_JobStatus] CHECK ([JobStatus] IN (0, 1, 2, 3, 4, 5)),
		CONSTRAINT [CK_WJbHistory_JobMore_ValidJson] CHECK ([JobMore] IS NULL OR isjson([JobMore]) = (1))
	) ON [PRIMARY]

	CREATE NONCLUSTERED INDEX [IX_WJbHistory_RuleId] ON [WJbHistory] ([RuleId] ASC)

	CREATE NONCLUSTERED INDEX [IX_WJbHistory_CreatedDesc] ON [WJbHistory] ([Created] DESC)
END
END

END

BEGIN /***** Cron Funcs *****/

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [CronPartValues] (@Input VARCHAR(20), @DefaultMin INT, @DefaultMax INT, @DefaultStep INT)
RETURNS VARCHAR(200)
AS
BEGIN
    DECLARE @Result VARCHAR(200);
	DECLARE @Min INT = @DefaultMin, @Max INT = @DefaultMax, @Step INT = @DefaultStep;

	IF @Input = ''*''
	BEGIN
		SET @Min = @DefaultMin
	END
    ELSE IF @Input LIKE ''*/%''
    BEGIN
        SET @Step = TRY_CAST(SUBSTRING(@Input, 3, LEN(@Input)) AS INT);
    END
    ELSE IF @Input LIKE ''%-%/%''
    BEGIN
        DECLARE @DashPos INT = CHARINDEX(''-'', @Input);
        DECLARE @SlashPos INT = CHARINDEX(''/'', @Input);

        SET @Min = TRY_CAST(SUBSTRING(@Input, 1, @DashPos - 1) AS INT);
        SET @Max = TRY_CAST(SUBSTRING(@Input, @DashPos + 1, @SlashPos - @DashPos - 1) AS INT);
        SET @Step = TRY_CAST(SUBSTRING(@Input, @SlashPos + 1, LEN(@Input)) AS INT);
    END
    ELSE IF @Input LIKE ''%/%''
    BEGIN
        DECLARE @SlashPos2 INT = CHARINDEX(''/'', @Input);
        SET @Min = TRY_CAST(SUBSTRING(@Input, 1, @SlashPos2 - 1) AS INT);
        SET @Step = TRY_CAST(SUBSTRING(@Input, @SlashPos2 + 1, LEN(@Input)) AS INT);
    END
    ELSE IF @Input LIKE ''%-%''
    BEGIN
        DECLARE @DashPos2 INT = CHARINDEX(''-'', @Input);
        SET @Min = TRY_CAST(SUBSTRING(@Input, 1, @DashPos2 - 1) AS INT);
        SET @Max = TRY_CAST(SUBSTRING(@Input, @DashPos2 + 1, LEN(@Input)) AS INT);
    END
    ELSE 
    BEGIN
        SET @Min = TRY_CAST(@Input AS INT);
        SET @Max = @Min;
    END

    ;WITH Numbers AS (
        SELECT @Min AS n
        UNION ALL
        SELECT n + @Step FROM Numbers WHERE n + @Step <= @Max
    )
    SELECT @Result = STRING_AGG(CAST(n AS VARCHAR), '','')
    FROM Numbers
    OPTION (MAXRECURSION 0);

    RETURN @Result
END
';

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [CronValidate] (@Expression varchar(100), @Now datetime)
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

    IF dbo.CronValidatePart(dbo.CronWord(@Expression, '' '', 1), DATEPART(MINUTE, @Now), 0, 59) = 0 RETURN 0;

    IF dbo.CronValidatePart(dbo.CronWord(@Expression, '' '', 2), DATEPART(HOUR, @Now), 0, 23) = 0 RETURN 0;

    IF dbo.CronValidatePart(dbo.CronWord(@Expression, '' '', 3), DATEPART(DAY, @Now), 1, 31) = 0 RETURN 0;

    IF dbo.CronValidatePart(dbo.CronWord(@Expression, '' '', 4), DATEPART(MONTH, @Now), 1, 12) = 0 RETURN 0;

    IF dbo.CronValidatePart(dbo.CronWord(@Expression, '' '', 5), dbo.CronWeekDay(@Now), 0, 6) = 0 RETURN 0;

    RETURN 1
END
';

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [CronValidatePart](@Expression varchar(100), @Value int, @Min int, @Max int)
RETURNS tinyint
AS
BEGIN
    IF @Expression LIKE ''%[^0-9*,/-]%'' RETURN 0
    IF @Value IS NULL OR @Min IS NULL OR @Max IS NULL OR NOT @Value BETWEEN @Min AND @Max RETURN 0  

    IF @Expression = ''*'' RETURN 1

	DECLARE @AllValues varchar(200) = (SELECT STRING_AGG([dbo].[CronPartValues](value, @Min, @Max, 1), '','') 
        FROM STRING_SPLIT(@Expression, '','') 
        WHERE LEN(value) > 0);

    IF CHARINDEX('','' + CAST(@Value as varchar) + '','', '','' + ISNULL(@AllValues, '''') + '','') > 0 RETURN 1

    RETURN 0
END
';

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [CronWeekDay](@Now datetime)
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
CREATE OR ALTER FUNCTION [CronWord] (@Words VARCHAR(100), @Separator VARCHAR(1), @Index INT)
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