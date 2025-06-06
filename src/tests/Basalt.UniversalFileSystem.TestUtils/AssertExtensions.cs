using System;
using System.Linq;
using System.Threading.Tasks;

namespace Basalt.UniversalFileSystem.TestUtils;

public static class AssertExtensions
{
    public static async Task ExpectException<T>(this Assert _, Func<Task> func) where T : Exception
    {
        bool caughtExpectedException = false;
        try
        {
            await func();
        }
        catch (Exception ex)
        {
            caughtExpectedException = ContainsException<T>(ex);
        }

        Assert.IsTrue(caughtExpectedException, "Expected exception is not caught");
    }

    // public static Task ExpectException(this Assert assert, Func<Task> func) => ExpectException<Exception>(assert, func);

    // public static void ExpectException<T>(this Assert assert, Action action) where T : Exception
    //     => assert.ExpectException(() => Task.Run(action));

    // public static void ExpectException(this Assert assert, Action action) => ExpectException<Exception>(assert, action);

    private static bool ContainsException<T>(Exception exception) where T : Exception
    {
        if (exception is T) return true;
        if (exception is AggregateException aggregateException)
            return aggregateException.InnerExceptions.Any(ContainsException<T>);

        if (exception.InnerException != null)
            return ContainsException<T>(exception.InnerException);
        return false;
    }
}