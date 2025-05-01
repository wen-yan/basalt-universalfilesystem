using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BasaltHexagons.UniversalFileSystem.TestUtils;

public static class AssertExtensions
{
    public static void ExpectException<T>(this Assert _, Func<Task> func) where T : Exception
    {
        bool caughtExpectedException = false;
        try
        {
            func().Wait();
        }
        catch (T)
        {
            caughtExpectedException = true;
        }
        catch (AggregateException ae)
        {
            caughtExpectedException = ae.InnerExceptions.Any(x => x is T);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e.Message);
        }

        Assert.IsTrue(caughtExpectedException, "Expected exception is not caught");
    }

    // public static void ExpectException(this Assert assert, Func<Task> func) => ExpectException<Exception>(assert, func);

    public static void ExpectException<T>(this Assert assert, Action action) where T : Exception
        => assert.ExpectException<T>(() => Task.Run(action));

    // public static void ExpectException(this Assert assert, Action action) => ExpectException<Exception>(assert, action);
}