# Basalt.UniversalFileSystem.AliyunOss

This package contains implementations for Aliyun OSS.

## Configurations

Except [common configuration](./Basalt.UniversalFileSystem.md#common-configurations), these configurations can be used.

| Configuration Key                                        | Type    | Required?                                                                            | Description                            |
|----------------------------------------------------------|---------|--------------------------------------------------------------------------------------|----------------------------------------|
| FileSystems:<fs-name>:Client                             |         | If missing, get `IOss` instance from dependency injection. See notes.                | Configuration root for creating `IOss` |
| FileSystems:<fs-name>:Client:Endpoint                    | string  | Required                                                                             | OSS endpoint                           |
| FileSystems:<fs-name>:Client:Credentials:Type            | string  | Required                                                                             | Supports `Basic`, `ConfigJsonProfile`  |
| FileSystems:<fs-name>:Client:Credentials:AccessKey       | string  | Required when Type = Basic                                                           | OSS access key Id                      |
| FileSystems:<fs-name>:Client:Credentials:SecretKey       | string  | Required when Type = Basic                                                           | OSS access secret                      |
| FileSystems:<fs-name>:Client:Credentials:SecurityToken   | string  | Optional when Type = Basic, default is `null`                                        | STS security token                     |
| FileSystems:<fs-name>:Client:Credentials:Profile         | string  | Optional when Type = ConfigJsonProfile, default is `current` value in ConfigJsonPath | Profile name                           |
| FileSystems:<fs-name>:Client:Credentials:ConfigJsonPath  | string  | Optional when Type = ConfigJsonProfile, default is `~/.aliyun/config.json`           | Profile name                           |
| FileSystems:<fs-name>:Settings:CreateBucketIfNotExists   | boolean | Optional, default is `false`                                                         | If create missing buckets              |

Notes:
- When `FileSystems:<fs-name>:Client` is missing, `IOss` is fetched/created from dependency injection using key `Basalt.UniversalFileSystem.AliyunOss.AliyunOssFileSystemFactory.CustomOssClient.<fs-name>`.
- Extension method `AddAliyunOssServiceClient` of `IServiceCollection` can be used to register custom `IOss` instance.

#### Configuration example

```yaml
FileSystems:
  Oss:
    UriRegexPattern: ^oss://.*$
    FileSystemFactoryClass: Basalt.UniversalFileSystem.AliyunOss.AliyunOssFileSystemFactory
    Client:
      Endpoint: https://oss-cn-shanghai.aliyuncs.com
      Credentials:
        Type: ConfigJsonProfile
```
