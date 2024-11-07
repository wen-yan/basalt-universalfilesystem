using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Krotus.UniversalFileSystem.Cli.Output;

interface IDatasetConsole
{
    ValueTask WriteAsync<T>(IAsyncEnumerable<T> dataset, CancellationToken cancellationToken);
}