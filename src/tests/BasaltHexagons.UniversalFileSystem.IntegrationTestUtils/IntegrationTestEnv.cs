using Microsoft.Extensions.Hosting;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTestUtils;

public class IntegrationTestEnv
{
    public IntegrationTestEnv()
    {
    }

    public IHost BuildHost(string? profiles, params string[] args)
    {
        BasaltHexagons.UniversalFileSystem.Bootstrap.AppHostBuilder hostBuilder = new(profiles);

        return hostBuilder.Build(args);
    }
}
