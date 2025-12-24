using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Basalt.UniversalFileSystem.Cli.Utils;

namespace Basalt.UniversalFileSystem.Cli.Output;

class ConsoleOutputWriter : IOutputWriter
{
    public ConsoleOutputWriter(IDatasetWriter datasetWriter)
    {
        this.DatasetWriter = datasetWriter;
    }

    private IDatasetWriter DatasetWriter { get; }

    public IAsyncDisposable SetColors(ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        ConsoleColor oldForeground = Console.ForegroundColor;
        ConsoleColor oldBackground = Console.BackgroundColor;

        ActionAsyncDisposable disposable = new(() =>
        {
            Console.ForegroundColor = oldForeground;
            Console.BackgroundColor = oldBackground;
            return ValueTask.CompletedTask;
        });

        if (foreground != null) Console.ForegroundColor = foreground.Value;
        if (background != null) Console.BackgroundColor = background.Value;

        return disposable;
    }

    public async ValueTask WriteAsync(string message, CancellationToken cancellationToken)
        => await Console.Out.WriteAsync(message.ToCharArray(), cancellationToken).ConfigureAwait(false);

    public ValueTask WriteDatasetAsync<T>(IAsyncEnumerable<T> dataset, CancellationToken cancellationToken)
        => this.DatasetWriter.WriteAsync(dataset, cancellationToken);
}