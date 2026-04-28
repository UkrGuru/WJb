using Microsoft.Extensions.DependencyInjection;
using WJb;
using WJb.Extensions;

namespace WJb.Tests;

public sealed class DiRegistration_SmokeTests
{
    [Fact]
    public void Services_Are_Registered()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddWJb();

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IActionFactory>());
        Assert.NotNull(provider.GetRequiredService<IJobQueue>());
        Assert.NotNull(provider.GetRequiredService<IJobProcessor>());
    }
}
