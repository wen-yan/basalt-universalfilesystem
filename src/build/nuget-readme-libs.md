This package is part of `Basalt.UniversalFileSystem` which is a uniform, cross‑cloud filesystem abstraction for .NET.

Access local files, Amazon S3, Azure Blob Storage, and Aliyun OSS through a single, consistent API — without vendor‑specific SDK complexity. `Basalt.UniversalFileSystem` is designed for developers who want a simple, reliable, and provider‑agnostic way to work with files across multiple storage backends.

`Basalt.UniversalFileSystem` includes these packages.

| Package name                         | Description                                                                      |
|--------------------------------------|----------------------------------------------------------------------------------|
| Basalt.UniversalFileSystem           | The interface to use UniversalFileSystem                                         |
| Basalt.UniversalFileSystem.Core      | Provider abstract interfaces and classes for implements of different filesystems |
| Basalt.UniversalFileSystem.File      | Local filesystem                                                                 |
| Basalt.UniversalFileSystem.AwsS3     | AWS S3                                                                           |
| Basalt.UniversalFileSystem.AzureBlob | Azure blob                                                                       |
| Basalt.UniversalFileSystem.AliyunOss | Aliyun OSS                                                                       |
| Basalt.UniversalFileSystem.Memory    | Memory filesystem. Can be used for testing                                       |


More information can be found from https://github.com/wen-yan/basalt-universalfilesystem
