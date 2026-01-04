# Basalt.UniversalFileSystem

This package contains interface `Basalt.UniversalFileSystem.IUniversalFileSystem` and `IServiceCollection` extension method `AddUniversalFileSystem()`.

- Interface `Basalt.UniversalFileSystem.IUniversalFileSystem` is the only entry to use `Basalt.UniversalFileSystem`. It provides methods for list, copy, move, delete operations.
- Extension method `AddUniversalFileSystem()` registers interface `Basalt.UniversalFileSystem.IUniversalFileSystem` to dependency injection using its default implementation with configurations.

## Common configurations
These configurations are necessary for all filesystem implementations. They are used to find implementations of `IFielSystemFactory` from URIs, which are used to create `IFileSystem` objects. 

| Configuration Key                             | Type   | Required? | Description                                                                                                                              |
|-----------------------------------------------|--------|-----------|------------------------------------------------------------------------------------------------------------------------------------------|
| FileSystems                                   |        | Required  | This is the configuration of all supported filesystem. It can contains multiple filesystem implementations which have different names.   |
| FileSystems:<fs-name>                         | string | Required  | Name of filesystem implementation, like `S3`, `File`. <fs-name> can be any word, but must be unique.                                     |
| FileSystems:<fs-name>:UriRegexPattern         | string | Required  | Regular expression pattern of URI. Default implementation of `IUniversalFileSystem` uses this to find the first matched implementation.  |
| FileSystems:<fs-name>:FileSystemFactoryClass  | string | Required  | Class full name of filesystem implementation factory in code, which is used to create implementation instance.                           |

## Example
This is an example of how to use `AddUniversalFileSystem()`, `IUniversalFileSystem` for different filesystems with different configurations.

##### Configurations
```yaml
UniversalFileSystem-Cli:
  UniversalFileSystem:
    FileSystems:

      File:  # Can be any name as long as it's unique.
        UriRegexPattern: ^file:///.*$   # It's not necessary to be `file`. It can be any scheme too, as long as regex pattern matches.
        FileSystemFactoryClass: Basalt.UniversalFileSystem.File.FileFileSystemFactory     # Implementation factory of local filesystem.

      S3-a:
        UriRegexPattern: ^s3://bucket-a/.*$  # Only for `bucket-a`
        FileSystemFactoryClass: Basalt.UniversalFileSystem.AwsS3.AwsS3FileSystemFactory   # Implementation factory of AWS S3.
        # Other configurations for S3. More details see docs/Basalt.UniversalFileSystem.AwsS3.md

      S3:
        UriRegexPattern: ^s3://.*$    # For other buckets. The order is important.
        FileSystemFactoryClass: Basalt.UniversalFileSystem.AwsS3.AwsS3FileSystemFactory
        # Other configurations for S3. More details see docs/Basalt.UniversalFileSystem.AwsS3.md
        # It could have different configurations from `S3-a`, for example different credentials.
```

##### Register `Basalt.UniversalFileSystem.IUniversalFileSystem` and filesystem implementations with configuration
```csharp
ServiceCollection BuildServices(ServiceCollection services)
{
    return services.
        // Register default implementation of `IUniversalFileSystem`.
        .AddUniversalFileSystem("UniversalFileSystem-Cli:UniversalFileSystem")  // The configuration key of `FileSystems` parent.  
        .AddFileFileSystem()    // Add local filesystem implementation
        .AddAwsS3FileSystem();  // Add S3 implementation
}
```

##### Usage of `Basalt.UniversalFileSystem.IUniversalFileSystem`
```csharp
async Task MyFunction(IIUniversalFileSystem ufs)
{
    await ufs.ListObjectsAsync(new Uri("s3://bucket-a/datasets/"), recursive = false);  // `S3-a` is used
    
    // Copy file between different filesystems.
    // Warning:
    //   File will be downloaded to local and uploaded to the destination if the configurations of source and dest URI are different.
    //   It could be slow when the file is large. MoveFileAsync() method has same behavior.
    await ufs.CopyFileAsync(
        sourceUri = new Uri("s3://bucket-b/datasets/a.zip"),
        destUri = new Uri("file:///local-datasets/b.zip"),
        overwrite = true);
}
```
