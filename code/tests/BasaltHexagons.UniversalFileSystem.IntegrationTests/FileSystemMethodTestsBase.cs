using System;

namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;


public interface IFileSystemMethodTests
{
    IUniversalFileSystem GetUniversalFileSystem();
}

public abstract class FileSystemMethodTestsBase
{
    protected IFileSystemMethodTests FileSystemMethodTests => (IFileSystemMethodTests)this;
    protected IUniversalFileSystem GetUniversalFileSystem() => this.FileSystemMethodTests.GetUniversalFileSystem();
}
