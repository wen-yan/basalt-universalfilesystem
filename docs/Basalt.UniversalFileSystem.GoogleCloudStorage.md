# Basalt.UniversalFileSystem.GoogleCloudStorage

This package contains implementations for Google Cloud Storage.

## Configurations

Except [common configuration](./Basalt.UniversalFileSystem.md#common-configurations), these configurations can be used.

| Configuration Key                                           | Type    | Required?                                                                       | Description                                                                      |
|-------------------------------------------------------------|---------|---------------------------------------------------------------------------------|----------------------------------------------------------------------------------|
| FileSystems:<fs-name>:Client                                |         | If missing, get `StorageClient` instance from dependency injection. See notes.  | Configuration root for creating `StorageClient`                                  |
| FileSystems:<fs-name>:Client:Credentials:Type               | string  | Required                                                                        | Supports `None`, `ApplicationDefault`, `FromFile`, `FromJson`, `FromAccessToken` |
| FileSystems:<fs-name>:Client:Credentials:CredentialFilePath | string  | Required when Type = FromFile                                                   | Credential file path                                                             |
| FileSystems:<fs-name>:Client:Credentials:CredentialJson     | string  | Required when Type = FromJson                                                   | Credentail json                                                                  |
| FileSystems:<fs-name>:Client:Credentials:AccessToken        | string  | Required when Type = FromAccessToken                                            | Access token                                                                     |
| FileSystems:<fs-name>:Client:Uri                            | string  | Optional, only required for emulators                                           | Service URI, for example, http://localhost:4443/storage/v1/                      |
| FileSystems:<fs-name>:Settings:ProjectId                    | string  | Required for creating buckets                                                   | Project ID                                                                       |
| FileSystems:<fs-name>:Settings:CreateBucketIfNotExists      | boolean | Optional, default is `false`                                                    | If create missing buckets                                                        |

Notes:
- When `FileSystems:<fs-name>:Client` is missing, `StorageClient` is fetched/created from dependency injection using key `Basalt.UniversalFileSystem.GoogleCloudStorage.GoogleCloudStorageFileSystemFactory.CustomStorageClient.<fs-name>`.
- Extension method `AddGoogleCloudStorageCustomClient` of `IServiceCollection` can be used to register custom `StorageClient` instance.

#### Configuration example

```yaml
FileSystems:
  Gs:
    UriRegexPattern: ^gs://.*$
    FileSystemFactoryClass: Basalt.UniversalFileSystem.GoogleCloudStorage.GoogleCloudStorageFileSystemFactory
    Client:
      Credentials:
        Type: None
      Uri: http://localhost:4443/storage/v1/
    Settings:
      ProjectId: test-project-id
      CreateBucketIfNotExists: true
```
