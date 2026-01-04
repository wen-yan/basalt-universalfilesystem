# Basalt.UniversalFileSystem.Cli

This is a command‑line tool built on top of `Basalt.UniversalFileSystem`. It allows different file systems to be accessed in a unified way. It also serves as an example demonstrating how to build applications or services using `Basalt.UniversalFileSystem`.

## How to install it
```bash
dotnet tool install --global Basalt.UniversalFileSystem.Cli --prerelease
```
Notes:
- Use option `--prerelease` to install pre-release version. It doesn't have release versions yet.


## Supported commands

- List files, `ufs ls`
- Show file content, `ufs cat`
- Copy files, `ufs cp`
- Move/rename file, `ufs mv`
- Delete file, `ufs rm`
- Create configuration template, `ufs config init`

## Configurations

Configuration file path is value of environment variable `UFS_CONFIG_PATH` or by default `~/.ufs/config.yaml`.

### Configuration example
```yaml
UniversalFileSystem-Cli:
  UniversalFileSystem:
    FileSystems:
        
      File:  # Local filesystem, can be any name as long as it's unique.
        UriRegexPattern: ^file:///.*$   # It's not necessary to be `file`. It can be any scheme too, as long as regex pattern matches.
        FileSystemFactoryClass: Basalt.UniversalFileSystem.File.FileFileSystemFactory     # Implementation factory of local filesystem.

      S3-a:
        UriRegexPattern: ^s3://bucket-a/.*$  # Only for `bucket-a`
        FileSystemFactoryClass: Basalt.UniversalFileSystem.AwsS3.AwsS3FileSystemFactory   # Implementation factory of AWS S3.
        Client:
          Credentials:
            Type: Basic
            AccessKey: abc
            SecretKey: xyz
          Options:
            Region: us-west-1

      S3:
        UriRegexPattern: ^s3://.*$    # For other buckets. The order is important.
        FileSystemFactoryClass: Basalt.UniversalFileSystem.AwsS3.AwsS3FileSystemFactory
        Client:
          Credentials:
            Type: Profile
            Profile: default
          Options:
            Region: us-west-1

      Oss:
        UriRegexPattern: ^oss://.*$
        FileSystemFactoryClass: Basalt.UniversalFileSystem.AliyunOss.AliyunOssFileSystemFactory
        Client:
          Endpoint: https://oss-cn-shanghai.aliyuncs.com
          Credentials:
            Type: ConfigJsonProfile
        Settings:
          CreateBucketIfNotExists: false
```

More information can be found from https://github.com/wen-yan/basalt-universalfilesystem.
