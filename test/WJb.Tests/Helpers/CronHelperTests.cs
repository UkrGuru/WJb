namespace WJb.Helpers.Tests;

public sealed class CronHelperTests
{
    [Fact]
    public void CronValidate_ReturnsFalse_WhenExpressionIsNullOrEmpty()
    {
        var now = new DateTime(2024, 1, 1, 12, 0, 0);

        Assert.False(CronHelper.CronValidate(null, now));
        Assert.False(CronHelper.CronValidate("", now));
        Assert.False(CronHelper.CronValidate("   ", now));
    }

    [Fact]
    public void CronValidate_ReturnsFalse_WhenFieldCountIsNotFive()
    {
        var now = DateTime.Now;

        Assert.False(CronHelper.CronValidate("* * * *", now));
        Assert.False(CronHelper.CronValidate("* * * * * *", now));
    }

    [Fact]
    public void CronValidate_Matches_WhenAllFieldsAreWildcard()
    {
        var now = new DateTime(2024, 5, 10, 14, 30, 0);

        Assert.True(CronHelper.CronValidate("* * * * *", now));
    }

    [Fact]
    public void CronValidate_Matches_SingleValues()
    {
        var now = new DateTime(2024, 5, 10, 14, 30, 0); // Fri

        Assert.True(CronHelper.CronValidate("30 14 10 5 5", now));
        Assert.False(CronHelper.CronValidate("31 14 10 5 5", now));
    }

    [Fact]
    public void CronValidate_Matches_CommaSeparatedValues()
    {
        var now = new DateTime(2024, 5, 10, 14, 30, 0);

        Assert.True(CronHelper.CronValidate("29,30,31 14 * * *", now));
        Assert.False(CronHelper.CronValidate("10,20 14 * * *", now));
    }

    [Fact]
    public void CronValidate_Matches_Ranges()
    {
        var now = new DateTime(2024, 5, 10, 14, 30, 0);

        Assert.True(CronHelper.CronValidate("30 13-15 * * *", now));
        Assert.False(CronHelper.CronValidate("30 15-18 * * *", now));
    }

    [Fact]
    public void CronValidate_ReturnsFalse_WhenRangeIsInvalid()
    {
        var now = DateTime.Now;

        Assert.False(CronHelper.CronValidate("30 20-10 * * *", now));
        Assert.False(CronHelper.CronValidate("30 a-b * * *", now));
    }

    [Fact]
    public void CronValidate_ReturnsFalse_ForInvalidToken()
    {
        var now = DateTime.Now;

        Assert.False(CronHelper.CronValidate("*/5 * * * *", now)); // steps not supported
        Assert.False(CronHelper.CronValidate("JAN * * * *", now)); // names not supported
    }
}