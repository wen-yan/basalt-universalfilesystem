namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class CopyObjectTests
{
    // [DataTestMethod]
    // [DynamicData(nameof(UniversalFileSystemStore.GetAllUniversalFileSystems), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    // public async Task CopyObject_ToSameDirectory(MethodTestsUniversalFileSystemWrapper ufs)
    // {
    //     // setup
    //     await ufs.PutObjectAsync("test.txt", "test content", false);
    //
    //     // test
    //     await ufs.MoveObjectAsync("test.txt", "test2.txt", false);
    //
    //     // verify
    //     Assert.IsFalse(await ufs.ExistsAsync("test.txt"));
    //     Assert.IsTrue(await ufs.ExistsAsync("test2.txt"));
    //
    //     ObjectMetadata? metadata = await ufs.GetObjectMetadataAsync("test2.txt");
    //     Assert.IsNotNull(metadata);
    //     Assert.AreEqual(ObjectType.File, metadata.ObjectType);
    //     Assert.AreEqual("test content".Length, metadata.ContentSize);
    //     Assert.IsNotNull(metadata.LastModifiedTimeUtc);
    //
    //     string content = await ufs.GetObjectAsync("test2.txt");
    //     Assert.AreEqual("test content", content);
    // }
}