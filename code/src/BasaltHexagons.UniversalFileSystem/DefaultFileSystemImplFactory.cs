using System;
using System.Collections.Generic;
using BasaltHexagons.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BasaltHexagons.UniversalFileSystem;

public class DefaultFileSystemImplFactory : IFileSystemImplFactory
{
    public DefaultFileSystemImplFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.ServiceProvider = serviceProvider;
        this.Configuration = configuration;
    }

    private IServiceProvider ServiceProvider { get; }
    private IConfiguration Configuration { get; }

    public IFileSystem Create(string scheme)
    {
        IConfigurationSection configurationSection = this.Configuration.GetSection(scheme);
        string? implementationClass = configurationSection["ImplementationClass"];
        if (implementationClass == null)
            throw new KeyNotFoundException(nameof(scheme));

        IFileSystemFactory factory = this.ServiceProvider.GetRequiredKeyedService<IFileSystemFactory>(implementationClass);

        IConfiguration implementationConfig = configurationSection.GetSection("ImplementationConfiguration");

        // Type? implementationType = AppDomain.CurrentDomain.GetAssemblies()
        //     .SelectMany(x => x.GetTypes())
        //     .SingleOrDefault(x => x.FullName == implementationClass);

        // if (implementationType == null)
        //     throw new ApplicationException();   // TODO


        return factory.Create(implementationConfig);
    }
}