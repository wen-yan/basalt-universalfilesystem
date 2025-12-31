# Basalt.UniversalFileSystem.Core

This package contains abstract interfaces, exceptions and utility methods for all filesystem implementations.

- Interface `IFileSystem`, it's the abstract interface of all filesystems. A typical implementation is using SDKs to access a filesystem, like AWS S3.
- Interface `IFileSystemFactory`, it's the abstract interface of `IFileSystem` factory. Its implementation should read configurations to create an instance of `IFileSystem`.

Note: Implementations of these twe interfaces should be stateless, so they wouldn't have race condition issues.

- Namespace `Basalt.UniversalFileSystem.Core.Exceptions` contains common exceptions.
- Namespace `Basalt.UniversalFileSystem.Core.Configuration` contains utility methods for reading configurations.
- Namespace `Basalt.UniversalFileSystem.Core.IO` contains utility classes for handling IO.

## How to support a new filesystem type

- Implement interface `IFileSystemFactory`. It should use passed configurations to create an instance of `IFileSystem`. Configurations could define credentials and/or options.
- Implement interface `IFileSystem`. Its implementation uses information/client built by the factory to access the filesystem.
- Provides `IServiceCollection` extension functions to register `IFileSystemFactory` implementation, so `Basalt.UniversalFileSystem.DefaultFileSystemStore` can create it.

Use project `Basalt.UniversalFileSystem.AwsS3` as a reference.
