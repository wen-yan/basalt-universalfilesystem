using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasaltHexagons.UniversalFileSystem.Core;
using BasaltHexagons.UniversalFileSystem.TestUtils;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class ListObjectsTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_InRoot(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("test1.txt", "test content 1", true);
        await ufs.PutFileAsync("test2.txt", "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("", false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("test1.txt", ObjectType.File, "test content 1".Length),
                ufs.MakeObjectMetadata("test2.txt", ObjectType.File, "test content 2".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_InSubFolder(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir/test1.txt", "test content 1", true);
        await ufs.PutFileAsync("dir/test2.txt", "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("", false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("dir/", ObjectType.Prefix, null, null)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_InSubFolderBothFileAndPrefix(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir1/dir2/test1.txt", "test content 1", true);
        await ufs.PutFileAsync("dir1/test2.txt", "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("dir1/", false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("dir1/dir2/", ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata("dir1/test2.txt", ObjectType.File, "test content 2".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_PrefixWithoutSlash(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir1/dir2/test1.txt", "test content 1", true);
        await ufs.PutFileAsync("dir1/test2.txt", "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("dir1", false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("dir1/", ObjectType.Prefix, null, null),
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_Recursive(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir1/dir2/test1.txt", "test content 1", true);
        await ufs.PutFileAsync("dir1/test2.txt", "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("dir1/", true)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("dir1/dir2/", ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata("dir1/dir2/test1.txt", ObjectType.File, "test content 1".Length),
                ufs.MakeObjectMetadata("dir1/test2.txt", ObjectType.File, "test content 2".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_RecursiveWithoutSlash(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir1/dir2/test1.txt", "test content 1", true);
        await ufs.PutFileAsync("dir1/test2.txt", "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("dir1", true)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("dir1/", ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata("dir1/dir2/", ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata("dir1/dir2/test1.txt", ObjectType.File, "test content 1".Length),
                ufs.MakeObjectMetadata("dir1/test2.txt", ObjectType.File, "test content 2".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_RecursiveMoreLevels(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir1/dir2/test1.txt", "test content 1", true);
        await ufs.PutFileAsync("dir1/dir3/test2.txt", "test content 2", true);
        await ufs.PutFileAsync("dir1/dir3/dir4/test3.txt", "test content 3", true);
        await ufs.PutFileAsync("dir1/test4.txt", "test content 4", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("dir1/", true)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("dir1/dir2/", ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata("dir1/dir3/", ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata("dir1/dir3/dir4/", ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata("dir1/dir2/test1.txt", ObjectType.File, "test content 1".Length),
                ufs.MakeObjectMetadata("dir1/dir3/test2.txt", ObjectType.File, "test content 2".Length),
                ufs.MakeObjectMetadata("dir1/dir3/dir4/test3.txt", ObjectType.File, "test content 3".Length),
                ufs.MakeObjectMetadata("dir1/test4.txt", ObjectType.File, "test content 3".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_HalfPrefix(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir1/dir2/test1.txt", "test content 1", true);
        await ufs.PutFileAsync("dir1/test4.txt", "test content 4", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("di", false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("dir1/", ObjectType.Prefix, null, null),
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_HalfFilename(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir1/test/test1.txt", "test content 1", true);
        await ufs.PutFileAsync("dir1/test4.txt", "test content 4", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("dir1/tes", false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("dir1/test/", ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata("dir1/test4.txt", ObjectType.File, "test content 4".Length),
            ],
            objects);
    }
    
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_PartialFilename(UniversalFileSystemTestWrapper ufs)
    {
        // setup
        await ufs.PutFileAsync("dir1/test/test1.txt", "test content 1", true);
        await ufs.PutFileAsync("dir1/test4.txt", "test content 4", true);
        await ufs.PutFileAsync("dir1/xxtest.txt", "test content 4", true);
        await ufs.PutFileAsync("dir1/yytest/test5.txt", "test content 5", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync("dir1/tes", false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata("dir1/test/", ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata("dir1/test4.txt", ObjectType.File, "test content 4".Length),
            ],
            objects);
    }

    private void AssertObjectMetadataListAreEquivalent(IEnumerable<ObjectMetadata> expected, IEnumerable<ObjectMetadata> actual)
    {
        CollectionAssert.That.AreEquivalent(expected, actual, new ObjectMetadataLastModifiedTimeUtcRangeEqualityComparer());
    }
}