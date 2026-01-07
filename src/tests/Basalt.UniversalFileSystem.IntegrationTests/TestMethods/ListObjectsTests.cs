using Basalt.UniversalFileSystem.Core;
using Basalt.UniversalFileSystem.TestUtils;

namespace Basalt.UniversalFileSystem.IntegrationTests.TestMethods;

[TestClass]
public class ListObjectsTests
{
    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_InRoot(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("test2.txt"), "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri(""), false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("test1.txt"), ObjectType.File, "test content 1".Length),
                ufs.MakeObjectMetadata(u.GetFullUri("test2.txt"), ObjectType.File, "test content 2".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_InSubFolder(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir/test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("dir/test2.txt"), "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri(""), false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("dir/"), ObjectType.Prefix, null, null)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_InSubFolderBothFileAndPrefix(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir1/dir2/test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/test2.txt"), "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri("dir1/"), false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir2/"), ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/test2.txt"), ObjectType.File, "test content 2".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_PrefixWithoutSlash(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir1/dir2/test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/test2.txt"), "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri("dir1"), false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/"), ObjectType.Prefix, null, null),
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_Recursive(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir1/dir2/test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/test2.txt"), "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri("dir1/"), true)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir2/"), ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir2/test1.txt"), ObjectType.File, "test content 1".Length),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/test2.txt"), ObjectType.File, "test content 2".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_RecursiveWithoutSlash(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir1/dir2/test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/test2.txt"), "test content 2", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri("dir1"), true)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/"), ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir2/"), ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir2/test1.txt"), ObjectType.File, "test content 1".Length),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/test2.txt"), ObjectType.File, "test content 2".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_RecursiveMoreLevels(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir1/dir2/test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/dir3/test2.txt"), "test content 2", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/dir3/dir4/test3.txt"), "test content 3", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/test4.txt"), "test content 4", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri("dir1/"), true)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir2/"), ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir3/"), ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir3/dir4/"), ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir2/test1.txt"), ObjectType.File, "test content 1".Length),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir3/test2.txt"), ObjectType.File, "test content 2".Length),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/dir3/dir4/test3.txt"), ObjectType.File, "test content 3".Length),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/test4.txt"), ObjectType.File, "test content 3".Length)
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_HalfPrefix(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir1/dir2/test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/test4.txt"), "test content 4", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri("di"), false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/"), ObjectType.Prefix, null, null),
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_HalfFilename(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir1/test/test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/test4.txt"), "test content 4", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri("dir1/tes"), false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/test/"), ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/test4.txt"), ObjectType.File, "test content 4".Length),
            ],
            objects);
    }

    [DataTestMethod]
    [DynamicData(nameof(UniversalFileSystemStore.GetSingleUniversalFileSystem), typeof(UniversalFileSystemStore), DynamicDataSourceType.Method)]
    public async Task ListObjects_PartialFilename(IUniversalFileSystem ufs, UriWrapper u)
    {
        // setup
        await ufs.PutFileAsync(u.GetFullUri("dir1/test/test1.txt"), "test content 1", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/test4.txt"), "test content 4", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/xxtest.txt"), "test content 4", true);
        await ufs.PutFileAsync(u.GetFullUri("dir1/yytest/test5.txt"), "test content 5", true);

        // test
        List<ObjectMetadata> objects = await ufs.ListObjectsAsync(u.GetFullUri("dir1/tes"), false)
            .OrderBy(x => x.Uri.ToString())
            .ToListAsync();

        // verify
        this.AssertObjectMetadataListAreEquivalent(
            [
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/test/"), ObjectType.Prefix, null, null),
                ufs.MakeObjectMetadata(u.GetFullUri("dir1/test4.txt"), ObjectType.File, "test content 4".Length),
            ],
            objects);
    }

    private void AssertObjectMetadataListAreEquivalent(IEnumerable<ObjectMetadata> expected, IEnumerable<ObjectMetadata> actual)
    {
        CollectionAssert.That.AreEquivalent(expected, actual, new ObjectMetadataLastModifiedTimeUtcRangeEqualityComparer());
    }
}