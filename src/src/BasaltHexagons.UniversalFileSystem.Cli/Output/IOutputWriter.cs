using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.Cli.Output;

interface IOutputWriter
{
    IAsyncDisposable SetColors(ConsoleColor? foreground = null, ConsoleColor? background = null);
    ValueTask WriteAsync(string message, CancellationToken cancellationToken);
    ValueTask WriteDatasetAsync<T>(IAsyncEnumerable<T> dataset, CancellationToken cancellationToken);
}

static class OutputWriterExtensions
{
    public static ValueTask WriteLineAsync(this IOutputWriter writer, string message, CancellationToken cancellationToken)
    {
        return writer.WriteAsync($"{message}{Environment.NewLine}", cancellationToken);
    }
}