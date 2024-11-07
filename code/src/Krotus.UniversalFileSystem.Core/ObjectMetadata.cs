using System;

namespace Krotus.UniversalFileSystem.Core;

public enum ObjectType
{
    File,
    Prefix
}

public sealed record ObjectMetadata(string Path, ObjectType ObjectType, long? ContentSize, DateTime? LastModifiedTime);