# Basalt.UniversalFileSystem.File

This package contains implementations for local filesystem.

## Configurations

`Basalt.UniversalFileSystem.File` doesn't need extra configurations. All it needs are [common configuration](./Basalt.UniversalFileSystem.md#common-configurations).

#### Configuration example

```yaml
FileSystems:
  File:
    UriRegexPattern: ^file:///.*$
    FileSystemFactoryClass: Basalt.UniversalFileSystem.File.FileFileSystemFactory
```
