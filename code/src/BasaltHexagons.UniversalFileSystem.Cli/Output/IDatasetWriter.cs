using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.Cli.Output;

interface IDatasetWriter
{
    ValueTask WriteAsync<T>(IAsyncEnumerable<T> dataset, CancellationToken cancellationToken);
}