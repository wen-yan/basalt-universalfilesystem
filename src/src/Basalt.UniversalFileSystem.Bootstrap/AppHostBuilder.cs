using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Basalt.UniversalFileSystem.Bootstrap;

public class AppHostBuilder
{
    private readonly string[] _profiles;

    public AppHostBuilder(string? profiles = null)
    {
        _profiles = profiles?.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? ["Production"];
    }

    public IHost Build(string[] args)
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

        hostBuilder
            .ConfigureAppConfiguration(this.InvokeConfigurators<AppConfigurationConfiguratorAttribute, IConfigurationBuilder>)
            .ConfigureServices(this.InvokeConfigurators<ServicesConfiguratorAttribute, IServiceCollection>);
        return hostBuilder.Build();
    }

    private void InvokeConfigurators<TAttribute, TArg>(HostBuilderContext context, TArg arg) where TAttribute : Attribute where TArg : class
    {
        IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.FullName?.StartsWith("Basalt.UniversalFileSystem") == true)
            .SelectMany(x => x.DefinedTypes)
            .SelectMany(x => x.DeclaredMethods)
            .Where(x => x is { IsStatic: true, IsGenericMethod: false })
            .Where(x =>
            {
                TAttribute? attribute = x.GetCustomAttributes(typeof(TAttribute), false)
                    .OfType<TAttribute>()
                    .FirstOrDefault();

                if (attribute == null)
                    return false;

                ProfilesAttribute? profilesAttribute = x.GetCustomAttributes(typeof(ProfilesAttribute), false)
                    .OfType<ProfilesAttribute>()
                    .FirstOrDefault();

                if ((profilesAttribute?.Profiles.Length ?? 0) == 0)
                    return true;

                return profilesAttribute!.Profiles.Any(profile => _profiles.Contains(profile));
            });
        foreach (MethodInfo method in methods)
            method.Invoke(null, [context, arg]);
    }
}