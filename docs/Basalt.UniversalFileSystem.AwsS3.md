# Basalt.UniversalFileSystem.AwsS3

This package contains implementations for AWS S3.

## Configurations

Except [common configuration](./Basalt.UniversalFileSystem.md#common-configurations), these configurations can be used.

| Configuration Key                                       | Type    | Required?                                                                  | Description                                         |
|---------------------------------------------------------|---------|----------------------------------------------------------------------------|-----------------------------------------------------|
| FileSystems:<fs-name>:Client                            |         | If missing, get `IAmazonS3` instance from dependency injection. See notes. | Configuration root for creating `IAmazonS3`         |
| FileSystems:<fs-name>:Client:Credentials:Type           | string  | Required                                                                   | Supports `Basic`, `EnvironmentVariables`, `Profile` |
| FileSystems:<fs-name>:Client:Credentials:AccessKey      | string  | Required when Type = Basic                                                 | Access key                                          |
| FileSystems:<fs-name>:Client:Credentials:SecretKey      | string  | Required when Type = Basic                                                 | Secret key                                          |
| FileSystems:<fs-name>:Client:Credentials:Profile        | string  | Optional when Type = Profile, default is `default`                         | Profile name                                        |
| FileSystems:<fs-name>:Client:Options:Region             | string  | Required or ServiceURL configured                                          | Region name                                         |
| FileSystems:<fs-name>:Client:Options:ServiceURL         | string  | Required or Region configured                                              | `AmazonS3Config.ServiceURL`                         |
| FileSystems:<fs-name>:Client:Options:ForcePathStyle     | boolean | Optional                                                                   | `AmazonS3Config.ForcePathStyle`                     |
| FileSystems:<fs-name>:Settings:CreateBucketIfNotExists  | boolean | Optional, default is `false`                                               | If create missing buckets                           |

Notes:
- When `FileSystems:<fs-name>:Client` is missing, `IAmazonS3` is fetched/created from dependency injection using key `Basalt.UniversalFileSystem.AwsS3.AwsS3FileSystemFactory.CustomS3Client.<fs-name>`.
- Extension method `AddAwsS3CustomClient` of `IServiceCollection` can be used to register custom `IAmazonS3` instance.
