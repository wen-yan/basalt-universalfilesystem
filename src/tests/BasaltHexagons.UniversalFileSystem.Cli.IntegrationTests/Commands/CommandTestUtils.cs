using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BasaltHexagons.UniversalFileSystem.Cli.IntegrationTests.Commands;

static class CommandTestUtils
{
    public static async Task<int> RunCommand(IHost host, params string[] args)
    {
        Parser parser = host.Services.GetRequiredService<Parser>();
        int exitCode = await parser.InvokeAsync(args);
        return exitCode;
    }
}