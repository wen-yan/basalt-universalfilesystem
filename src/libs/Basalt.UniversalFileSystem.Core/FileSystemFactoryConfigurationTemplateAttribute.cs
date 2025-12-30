using System;

namespace Basalt.UniversalFileSystem.Core;

/// <summary>
/// Attribution of Filesystem factory configuration template.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class FileSystemFactoryConfigurationTemplateAttribute : Attribute
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="configurationTemplate">Configuration template.</param>
    public FileSystemFactoryConfigurationTemplateAttribute(string configurationTemplate)
    {
        this.ConfigurationTemplate = configurationTemplate;
    }

    /// <summary>
    /// Configuration template of filesystem factory.
    /// </summary>
    public string ConfigurationTemplate { get; }
}