namespace Basalt.UniversalFileSystem.Core;

using System;
using System.Runtime.CompilerServices;

public readonly struct ContinueOnAnyAsyncMethodBuilder
{
    private ContinueOnAnyAsyncMethodBuilder(AsyncTaskMethodBuilder builder)
    {
        this.Builder = builder;
    }

    private AsyncTaskMethodBuilder Builder { get; }

    public static ContinueOnAnyAsyncMethodBuilder Create()
    {
        return new ContinueOnAnyAsyncMethodBuilder(AsyncTaskMethodBuilder.Create());
    }

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        this.Builder.Start(ref stateMachine);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        this.Builder.SetStateMachine(stateMachine);
    }

    public void SetResult()
    {
        this.Builder.SetResult();
    }

    public void SetException(Exception exception)
    {
        this.Builder.SetException(exception);
    }

    public ConfiguredTaskAwaitable Task => this.Builder.Task.ConfigureAwait(false);
}