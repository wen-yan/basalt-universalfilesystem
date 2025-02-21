using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public static class UniversalFileSystemAssert
{
    public static void VerifyObject(UniversalFileSystemTestWrapper ufs, string path, ObjectType objectType, string? content)
    {
        ObjectMetadata? actualMetadata = ufs.GetObjectMetadataAsync(path).Result;
        Assert.IsNotNull(actualMetadata);

        ObjectMetadata expectedMetadata = objectType == ObjectType.File
            ? ufs.MakeObjectMetadata(path, objectType, content?.Length)
            : ufs.MakeObjectMetadata(path, objectType, content?.Length, null);
        Assert.AreEqual(expectedMetadata, actualMetadata, new ObjectMetadataLastModifiedTimeUtcRangeEqualityComparer());

        if (objectType == ObjectType.File) Assert.AreEqual(content, ufs.GetObjectAsync(path).Result);
    }
}