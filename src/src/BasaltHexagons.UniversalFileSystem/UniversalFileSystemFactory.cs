using System;
using System.Runtime.CompilerServices;
using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;

namespace BasaltHexagons.UniversalFileSystem;

[AsyncMethodBuilder(typeof(ContinueOnAnyAsyncMethodBuilder))]
public static class UniversalFileSystemFactory
{
    public static IUniversalFileSystem Create(IServiceProvider serviceProvider, IConfiguration configuration)
        => new UniversalFileSystem(new DefaultFileSystemCreator(serviceProvider, configuration));
}