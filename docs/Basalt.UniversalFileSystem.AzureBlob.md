# Basalt.UniversalFileSystem.AzureBlob

This package contains implementations for Azure blob.

## Configurations

Except [common configuration](./Basalt.UniversalFileSystem.md#common-configurations), these configurations can be used.


| Configuration Key                                              | Type    | Required?                                                                           | Description                                                                                                        |
|----------------------------------------------------------------|---------|-------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------|
| FileSystems:<fs-name>:Client                                   |         | If missing, get `BlobServiceClient` instance from dependency injection. See notes.  | Configuration root for creating `BlobServiceClient`                                                                |
| FileSystems:<fs-name>:Client:ServiceUri                        | string  | Required                                                                            | A Uri referencing the blob service. This is likely to be similar to "https://{account_name}.blob.core.windows.net" |
| FileSystems:<fs-name>:Client:Credentials:Type                  | string  | Required                                                                            | Supports `DefaultAzure`, `StorageSharedKey`                                                                        |
| FileSystems:<fs-name>:Client:Credentials:AccountName           | string  | Required when Type = StorageSharedKey                                               | Storage Account                                                                                                    |
| FileSystems:<fs-name>:Client:Credentials:SecretKey             | string  | Required when Type = Basic                                                          | Storage Account access key                                                                                         |
| FileSystems:<fs-name>:Settings:CreateBlobContainerIfNotExists  | boolean | Optional, default is `false`                                                        | If create missing containers                                                                                       |

Notes:
- When `FileSystems:<fs-name>:Client` is missing, `BlobServiceClient` is fetched/created from dependency injection using key `Basalt.UniversalFileSystem.AzureBlob.AzureBlobFileSystemFactory.CustomBlobServiceClient.<fs-name>`.
- Extension method `AddAzureBlobCustomClient` of `IServiceCollection` can be used to register custom `BlobServiceClient` instance.

#### Configuration example

```yaml
FileSystems:
  AzureBlob:
    UriRegexPattern: ^azurite://.*$
    FileSystemFactoryClass: Basalt.UniversalFileSystem.AzureBlob.AzureBlobFileSystemFactory
    Client:
      # configurations for Azurite
      ServiceUri: http://localhost:10000/account1
      Credentials:
        Type: StorageSharedKey
        AccountName: devstoreaccount1
        AccountKey: Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==
    Settings:
      CreateBlobContainerIfNotExists: false
```
