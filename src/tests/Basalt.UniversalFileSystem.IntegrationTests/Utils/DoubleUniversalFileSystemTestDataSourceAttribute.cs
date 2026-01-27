using System.Reflection;

namespace Basalt.UniversalFileSystem.IntegrationTests.Utils;

public class DoubleUniversalFileSystemTestDataSourceAttribute : Attribute, ITestDataSource
{
    public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
    {
        IUniversalFileSystem ufs = UniversalFileSystemUtils.GetUniversalFileSystem();

        var firstUriWrappers = UriWrapper.NonMemoryUriWrappers.Select(x => (x, UriWrapper.Memory));
        var secondUriWrappers = UriWrapper.NonMemoryUriWrappers.Select(x => (UriWrapper.Memory, x));
        var selfUriWrappers = UriWrapper.AllUriWrappers.Select(x => (x, x));

        return firstUriWrappers.Concat(secondUriWrappers).Concat(selfUriWrappers)
            .Select(x => new object[] { ufs, x.Item1, x.Item2 });
    }

    public string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
    {
        return $"{methodInfo.Name}({string.Join(", ", data ?? Array.Empty<object?>())})";
    }
}