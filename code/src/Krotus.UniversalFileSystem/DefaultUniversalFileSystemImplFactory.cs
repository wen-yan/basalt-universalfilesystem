using System;
using System.Linq;
using Krotus.UniversalFileSystem.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Krotus.UniversalFileSystem;

public class DefaultUniversalFileSystemImplFactory : IUniversalFileSystemImplFactory
{
    public DefaultUniversalFileSystemImplFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.ServiceProvider = serviceProvider;
        this.Configuration = configuration;
    }
    private IServiceProvider ServiceProvider { get; }
    private IConfiguration Configuration { get; }
    
    public IFileSystemImpl Create(string scheme)
    {
        IConfigurationSection configurationSection = this.Configuration.GetSection(scheme);
        string? implementationClass = configurationSection["ImplementationClass"];
        if (implementationClass == null)
            throw new ArgumentOutOfRangeException(nameof(scheme));  // TODO

        IConfiguration implementationConfig = configurationSection.GetSection("ImplementationConfiguration");

        Type? implementationType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .SingleOrDefault(x => x.FullName == implementationClass);

        if (implementationType == null)
            throw new ApplicationException();   // TODO

        IFileSystemImplCreator implCreator = this.ServiceProvider.GetRequiredKeyedService<IFileSystemImplCreator>(implementationType);
        return implCreator.Create(implementationConfig);
    }
}