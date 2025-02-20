using System;
using System.Collections.Generic;
using BasaltHexagons.UniversalFileSystem.Core;

namespace BasaltHexagons.UniversalFileSystem.TestUtils;

public class ObjectMetadataLastModifiedTimeUtcRangeEqualityComparer : IEqualityComparer<ObjectMetadata>
{
    public ObjectMetadataLastModifiedTimeUtcRangeEqualityComparer(TimeSpan timeTolerance = default)
    {
        this.TimeTolerance = timeTolerance == default ? TimeSpan.FromMinutes(5) : timeTolerance;
    }

    public TimeSpan TimeTolerance { get; }

    public bool Equals(ObjectMetadata? x, ObjectMetadata? y)
    {
        if (x == null && y == null) return true;
        if ((x == null) != (y == null)) return false;
        if (object.ReferenceEquals(x, y)) return true;

        if (x!.Path != y!.Path) return false;
        if (x.ObjectType != y.ObjectType) return false;

        if ((x.ContentSize == null) != (y.ContentSize == null)) return false;
        if (x.ContentSize != y.ContentSize) return false;

        if ((x.LastModifiedTimeUtc == null) != (y.LastModifiedTimeUtc == null)) return false;

        if (x.LastModifiedTimeUtc - y.LastModifiedTimeUtc > this.TimeTolerance) return false;
        if (y.LastModifiedTimeUtc - x.LastModifiedTimeUtc > this.TimeTolerance) return false;

        return true;
    }

    public int GetHashCode(ObjectMetadata obj)
    {
        throw new System.NotImplementedException();
    }
}