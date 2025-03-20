using System;

namespace BasaltHexagons.UniversalFileSystem.Core;

public enum ObjectType
{
    File,
    Prefix
}

public sealed record ObjectMetadata(Uri Uri, ObjectType ObjectType, long? ContentSize, DateTime? LastModifiedTimeUtc);