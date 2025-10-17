BEGIN /***** Init Tables *****/

BEGIN /*** Init WJbActions ***/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[WJbActions]') AND type in (N'U'))
BEGIN
	CREATE TABLE [WJbActions] (
		[ActionId] [int] IDENTITY(1000,1) NOT NULL,
		[ActionName] [nvarchar](100) NOT NULL,
		[ActionType] [nvarchar](255) NOT NULL,
		[ActionMore] [nvarchar](2000) NULL,
		[Disabled] [bit] NOT NULL DEFAULT (0),
		CONSTRAINT [PK_WJbActions] PRIMARY KEY CLUSTERED ([ActionId] ASC),
		CONSTRAINT [UX_WJbActions_ActionName] UNIQUE NONCLUSTERED ([ActionName] ASC),
		CONSTRAINT [CK_WJbActions_ActionMore_ValidJson] CHECK ([ActionMore] IS NULL  OR isjson([ActionMore]) = (1) OR TRY_CAST([ActionMore] AS UNIQUEIDENTIFIER) IS NOT NULL)
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
		[RuleMore] [nvarchar](2000) NULL,
	 CONSTRAINT [PK_WJbRules] PRIMARY KEY CLUSTERED ([RuleId] ASC),
	 CONSTRAINT [UX_WJbRules_RuleName] UNIQUE NONCLUSTERED ([RuleName] ASC),
	 CONSTRAINT [FK_WJbRules_WJbActions] FOREIGN KEY([ActionId]) REFERENCES [dbo].[WJbActions] ([ActionId]),
	 CONSTRAINT [CK_WJbRules_RuleMore_ValidJson] CHECK ([RuleMore] IS NULL OR isjson([RuleMore]) = (1) OR TRY_CAST([RuleMore] AS UNIQUEIDENTIFIER) IS NOT NULL)
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
        [JobMore] [nvarchar](2000) NULL,
        [JobStatus] [tinyint] NOT NULL DEFAULT (0),
        CONSTRAINT [PK_WJbQueue] PRIMARY KEY CLUSTERED ([JobId] ASC),
        CONSTRAINT [FK_WJbQueue_WJbRules] FOREIGN KEY ([RuleId]) REFERENCES [WJbRules] ([RuleId]),
        CONSTRAINT [CK_WJbQueue_JobStatus] CHECK ([JobStatus] IN (0, 1, 2, 3, 4, 5)),
        CONSTRAINT [CK_WJbQueue_JobMore_ValidJson] CHECK ([JobMore] IS NULL OR isjson([JobMore]) = (1) OR TRY_CAST([JobMore] AS UNIQUEIDENTIFIER) IS NOT NULL)
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
		[JobMore] [nvarchar](2000) NULL,
		[JobStatus] [tinyint] NOT NULL,
		CONSTRAINT [PK_WJbHistory] PRIMARY KEY CLUSTERED ([JobId] DESC),
		CONSTRAINT [FK_WJbHistory_WJbRules] FOREIGN KEY ([RuleId]) REFERENCES [WJbRules] ([RuleId]),
		CONSTRAINT [CK_WJbHistory_JobStatus] CHECK ([JobStatus] IN (0, 1, 2, 3, 4, 5)),
		CONSTRAINT [CK_WJbHistory_JobMore_ValidJson] CHECK ([JobMore] IS NULL OR isjson([JobMore]) = (1) OR TRY_CAST([JobMore] AS UNIQUEIDENTIFIER) IS NOT NULL)
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
CREATE OR ALTER FUNCTION [CronMax] (@expression VARCHAR(20), @default INT)
RETURNS INT
AS
BEGIN
    DECLARE @max INT = NULL;

    DECLARE @slashPos int = CHARINDEX(''/'', @expression);
    IF @slashPos > 0 SET @expression = LEFT(@expression, @slashPos - 1)

    DECLARE @dashPos int = CHARINDEX(''-'', @expression)
    SET @max = IIF(@dashPos > 0, TRY_CAST(SUBSTRING(@expression, @dashPos + 1, LEN(@expression)) AS INT), @default);

    RETURN IIF(@max < @default, @max, @default);
END
';

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [CronMin] (@expression VARCHAR(20), @default INT)
RETURNS INT
AS
BEGIN
    DECLARE @min INT = NULL;

    DECLARE @dashPos int = CHARINDEX(''-'', @expression)
    IF @dashPos > 0
        SET @min = TRY_CAST(LEFT(@expression, @dashPos - 1) AS INT);
    ELSE 
    BEGIN
        DECLARE @slashPos int = CHARINDEX(''/'', @expression);
        IF @slashPos > 0
            SET @min = TRY_CAST(LEFT(@expression, @slashPos - 1) AS INT);
        ELSE
            SET @min = TRY_CAST(@expression AS INT);
    END

    RETURN IIF(@min > @default, @min, @default);
END
';

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [CronStep] (@expression VARCHAR(20))
RETURNS INT
AS
BEGIN
    DECLARE @step INT = NULL, @default INT = 1;

    DECLARE @slashPos int = CHARINDEX(''/'', @expression);
    IF @slashPos > 0 SET @step = TRY_CAST(SUBSTRING(@expression, @slashPos + 1, LEN(@expression)) AS INT);

    RETURN IIF(@step > @default, @step, @default);
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

    IF dbo.CronValidateParts(dbo.CronWord(@Expression, '' '', 1), DATEPART(MINUTE, @Now), 0, 59) = 0 RETURN 0;

    IF dbo.CronValidateParts(dbo.CronWord(@Expression, '' '', 2), DATEPART(HOUR, @Now), 0, 23) = 0 RETURN 0;

    IF dbo.CronValidateParts(dbo.CronWord(@Expression, '' '', 3), DATEPART(DAY, @Now), 1, 31) = 0 RETURN 0;

    IF dbo.CronValidateParts(dbo.CronWord(@Expression, '' '', 4), DATEPART(MONTH, @Now), 1, 12) = 0 RETURN 0;

    IF dbo.CronValidateParts(dbo.CronWord(@Expression, '' '', 5), dbo.CronWeekDay(@Now), 0, 6) = 0 RETURN 0;

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
    IF @Expression = ''*'' RETURN 1

    IF CHARINDEX(''-'', @Expression, 0) > 0 OR CHARINDEX(''/'', @Expression, 0) > 0 BEGIN 
        DECLARE @Current int = dbo.CronMin(@Expression, @Min);
        DECLARE @ExpMax int  = dbo.CronMax(@Expression, @Max);
        DECLARE @ExpStep int = dbo.CronStep(@Expression);

        WHILE @Current <= @ExpMax
        BEGIN
            IF @Current = @Value RETURN 1
            SET @Current += @ExpStep;
        END
    END
    ELSE IF TRY_CAST(@Expression as int) = @Value 
        RETURN 1

    RETURN 0
END
';

EXEC dbo.sp_executesql @statement = N'
-- ==============================================================
-- Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
-- ==============================================================
CREATE OR ALTER FUNCTION [CronValidateParts](@Expression varchar(100), @Value int, @Min int, @Max int)
RETURNS tinyint
AS
BEGIN
    IF @Value IS NULL OR @Min IS NULL OR @Max IS NULL OR NOT @Value BETWEEN @Min AND @Max RETURN 0  

    DECLARE @PosComma int = CHARINDEX('','', @Expression) 
    WHILE @PosComma > 0 OR LEN(@Expression) > 0
    BEGIN
        IF dbo.CronValidatePart(IIF(@PosComma > 0, LEFT(@Expression, @PosComma - 1), @Expression), @Value, @Min, @Max) = 1 RETURN 1;

        SET @Expression = IIF(@PosComma > 0, SUBSTRING(@Expression, @PosComma + 1, LEN(@Expression)), '''');
        SET @PosComma = CHARINDEX('','', @Expression);
    END
    RETURN 0;
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