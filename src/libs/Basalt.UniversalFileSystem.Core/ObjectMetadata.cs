using System;

namespace Basalt.UniversalFileSystem.Core;

/// <summary>
/// Filesystem object type enum.
/// </summary>
public enum ObjectType
{
    /// <summary>
    /// File type.
    /// </summary>
    File,
    /// <summary>
    /// Prefix (directory) type.
    /// </summary>
    Prefix
}

/// <summary>
/// Metadata of filesystem object.
/// </summary>
/// <param name="Uri">Object URI.</param>
/// <param name="ObjectType">Object type.</param>
/// <param name="ContentSize">Object (file) content size.</param>
/// <param name="LastModifiedTimeUtc">Object (file) last modified UTC time.</param>
public sealed record ObjectMetadata(Uri Uri, ObjectType ObjectType, long? ContentSize, DateTime? LastModifiedTimeUtc);