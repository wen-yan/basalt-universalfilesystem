using System.Reflection;


namespace Basalt.UniversalFileSystem.IntegrationTests.Utils;

class SingleUniversalFileSystemTestDataSourceAttribute(params string[]? uriWrapperNames) : Attribute, ITestDataSource
{
    public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
    {
        IUniversalFileSystem ufs = UniversalFileSystemUtils.GetUniversalFileSystem();
        return UriWrapper.AllUriWrappers
            .Where(x => uriWrapperNames == null || uriWrapperNames.Length == 0 || uriWrapperNames.Contains(x.Name))
            .Select(x => new object[] { ufs, x });
    }

    public string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
    {
        return $"{methodInfo.Name}({string.Join(", ", data ?? Array.Empty<object?>())})";
    }
}