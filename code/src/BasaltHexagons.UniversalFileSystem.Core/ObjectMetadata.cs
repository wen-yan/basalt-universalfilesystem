using System;

namespace BasaltHexagons.UniversalFileSystem.Core;

public enum ObjectType
{
    File,
    Prefix
}

public sealed record ObjectMetadata(Uri Path, ObjectType ObjectType, long? ContentSize, DateTime? LastModifiedTime);