using System;

namespace Krotus.UniversalFileSystem.Core;

public sealed record ObjectMetadata(string Path, bool IsFile, long? ContentSize, DateTime? LastModifiedTime);