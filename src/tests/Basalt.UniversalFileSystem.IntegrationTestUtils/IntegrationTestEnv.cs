using Microsoft.Extensions.Hosting;

namespace Basalt.UniversalFileSystem.IntegrationTestUtils;

public class IntegrationTestEnv
{
    public IntegrationTestEnv()
    {
    }

    public IHost BuildHost(string? profiles, params string[] args)
    {
        Basalt.UniversalFileSystem.Bootstrap.AppHostBuilder hostBuilder = new(profiles);

        return hostBuilder.Build(args);
    }
}
