using System;

namespace Basalt.UniversalFileSystem.Cli.Bootstrap;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal class ProfilesAttribute : Attribute
{
    public ProfilesAttribute(params string[] profiles)
    {
        this.Profiles = profiles;
    }
    public string[] Profiles { get; }
}