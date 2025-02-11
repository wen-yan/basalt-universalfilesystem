using System;
using System.Collections.Generic;

using BasaltHexagons.UniversalFileSystem.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem;

class DefaultFileSystemCreator : IFileSystemCreator
{
    public DefaultFileSystemCreator(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.ServiceProvider = serviceProvider;
        this.Configuration = configuration;
    }

    private IServiceProvider ServiceProvider { get; }
    private IConfiguration Configuration { get; }

    public IFileSystem Create(string scheme)
    {
        IConfigurationSection configurationSection = this.Configuration.GetSection($"BasaltHexagons:UniversalFileSystem:Schemes:{scheme}");

        string? implementationFactoryClass = configurationSection["ImplementationFactoryClass"];
        if (implementationFactoryClass == null)
            throw new KeyNotFoundException(nameof(scheme));

        IFileSystemFactory factory = this.ServiceProvider.GetRequiredKeyedService<IFileSystemFactory>(implementationFactoryClass);
        IConfiguration implementationConfig = configurationSection.GetSection("ImplementationConfiguration");

        return new FileSystemWrapper(factory.Create(implementationConfig));
    }
}