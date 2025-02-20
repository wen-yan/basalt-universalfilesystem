using System;
using System.Collections.Generic;
using System.Linq;

namespace BasaltHexagons.UniversalFileSystem.TestUtils;

public static class EnumerableAssert
{
    public static void AreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual) where T : notnull
        => AreEquivalent(expected, actual, EqualityComparer<T>.Default);

    public static void AreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer) where T : notnull
    {
        List<T> expectedList = expected.ToList();
        List<T> actualList = actual.ToList();

        for (int i = expectedList.Count - 1; i >= 0 && expectedList.Count > 0 && actualList.Count > 0; --i)
        {
            T item = expectedList[i];
            int index = -1;

            for (int j = 0; j < actualList.Count; ++j)
            {
                if (comparer.Equals(item, actualList[j]))
                {
                    index = j;
                    break;
                }
            }

            if (index >= 0)
            {
                expectedList.RemoveAt(i);
                actualList.RemoveAt(index);
            }
        }

        if (expectedList.Count > 0 || actualList.Count > 0)
        {
            Assert.Fail($"""
                         Items are not matched.
                         expected:
                         {string.Join(Environment.NewLine, expectedList)}
                         actual:
                         {string.Join(Environment.NewLine, actualList)}");
                         """);
        }
    }
}