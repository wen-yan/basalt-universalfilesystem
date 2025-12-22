using System;
using System.Threading.Tasks;

namespace Basalt.UniversalFileSystem.Cli.Utils;

public class ActionAsyncDisposable : IAsyncDisposable
{
    public ActionAsyncDisposable(Func<ValueTask> action)
    {
        this.DisposeAction = action;
    }

    public Func<ValueTask> DisposeAction { get; }


    public ValueTask DisposeAsync() => this.DisposeAction();
}