# Basalt.UniversalFileSystem.Sftp

This package contains implementations for SFTP.

## Configurations

Except [common configuration](./Basalt.UniversalFileSystem.md#common-configurations), these configurations can be used.

| Configuration Key                      | Type    | Required?                                                                   | Description                                   |
|----------------------------------------|---------|-----------------------------------------------------------------------------|-----------------------------------------------|
| FileSystems:<fs-name>:Client           |         | If missing, get `SftpClient` instance from dependency injection. See notes. | Configuration root for creating `SftpClient`  |
| FileSystems:<fs-name>:Client:Host      | string  | Required                                                                    | Connection host name or address               |
| FileSystems:<fs-name>:Client:Port      | string  | Required                                                                    | Connection host port                          |
| FileSystems:<fs-name>:Client:Username  | string  | Required                                                                    | Authentication username                       |
| FileSystems:<fs-name>:Client:Password  | string  | Required                                                                    | Authentication password                       |

Notes:
- When `FileSystems:<fs-name>:Client` is missing, `SftpClient` is fetched/created from dependency injection using key `Basalt.UniversalFileSystem.Sftp.SftpFileSystemFactory.CustomSftpClient.<fs-name>`.
- Extension method `AddSftpCustomClient` of `IServiceCollection` can be used to register custom `SftpClient` instance.

#### Configuration example

```yaml
FileSystems:
  Sftp:
    UriRegexPattern: ^sftp://localhost/.*$
    FileSystemFactoryClass: Basalt.UniversalFileSystem.Sftp.SftpFileSystemFactory
    Client:
      Host: localhost
      Port: 2222
      Username: demo
      Password: demo
```
