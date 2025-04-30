using System;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public static class UniversalFileSystemAssert
{
    public static void VerifyObject(UniversalFileSystemTestWrapper ufs, string uri, ObjectType objectType, string? content)
    {
        ObjectMetadata actualMetadata = ufs.GetFileMetadataAsync(uri).Result;
        Assert.IsNotNull(actualMetadata);

        VerifyObject(ufs, uri, objectType, content, actualMetadata);
    }

    public static void VerifyObject(UniversalFileSystemTestWrapper ufs, string uri, ObjectType objectType, string? content, ObjectMetadata? actualMetadata)
    {
        ObjectMetadata expectedMetadata = objectType == ObjectType.File
            ? ufs.MakeObjectMetadata(uri, objectType, content?.Length)
            : ufs.MakeObjectMetadata(uri, objectType, content?.Length, null);
        Assert.AreEqual(expectedMetadata, actualMetadata, new ObjectMetadataLastModifiedTimeUtcRangeEqualityComparer());

        if (objectType == ObjectType.File) Assert.AreEqual(content, ufs.GetFileAsync(uri).Result);
    }
}