using System;

namespace Basalt.UniversalFileSystem.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class FileSystemFactoryConfigurationTemplateAttribute : Attribute
{
    public FileSystemFactoryConfigurationTemplateAttribute(string configurationTemplate)
    {
        this.ConfigurationTemplate = configurationTemplate;
    }

    public string ConfigurationTemplate { get; }
}