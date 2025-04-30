using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

static class UniversalFileSystemTestExtensions
{
    public static async Task InitializeAsync(this IUniversalFileSystem ufs, params UriWrapper[] uriWrappers)
    {
        foreach (UriWrapper uriWrapper in uriWrappers)
        {
            await ufs.InitializeAsync(uriWrapper);
        }
    }

    public static async Task InitializeAsync(this IUniversalFileSystem ufs, UriWrapper uriWrapper)
    {
        // delete all files
        IAsyncEnumerable<ObjectMetadata> allFiles = ufs.ListObjectsAsync(uriWrapper.GetFullUri(""), true);
        await foreach (ObjectMetadata file in allFiles)
            await ufs.DeleteFileAsync(file.Uri);

        if (uriWrapper.BaseUri.Scheme.StartsWith("file"))
        {
            string root = uriWrapper.BaseUri.LocalPath;

            // Delete all files
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    public static ObjectMetadata MakeObjectMetadata(this IUniversalFileSystem ufs, Uri uri, ObjectType objectType, long? contentSize, DateTime? lastModifiedTimeUtc)
        => new(uri, objectType, contentSize, lastModifiedTimeUtc);

    public static ObjectMetadata MakeObjectMetadata(this IUniversalFileSystem ufs, Uri uri, ObjectType objectType, long? contentSize)
        => ufs.MakeObjectMetadata(uri, objectType, contentSize, DateTime.UtcNow);

    public static void VerifyObject(this IUniversalFileSystem ufs, Uri uri, ObjectType objectType, string? content, ObjectMetadata? actualMetadata)
    {
        ObjectMetadata expectedMetadata = objectType == ObjectType.File
            ? new ObjectMetadata(uri, objectType, content?.Length, DateTime.UtcNow)
            : new ObjectMetadata(uri, objectType, content?.Length, null);
        Assert.AreEqual(expectedMetadata, actualMetadata, new ObjectMetadataLastModifiedTimeUtcRangeEqualityComparer());

        if (objectType == ObjectType.File)
            Assert.AreEqual(content, ufs.GetFileStringAsync(uri).Result);
    }

    public static void VerifyObject(this IUniversalFileSystem ufs, Uri uri, ObjectType objectType, string? content)
    {
        ObjectMetadata actualMetadata = ufs.GetFileMetadataAsync(uri).Result;
        Assert.IsNotNull(actualMetadata);

        ufs.VerifyObject(uri, objectType, content, actualMetadata);
    }
}