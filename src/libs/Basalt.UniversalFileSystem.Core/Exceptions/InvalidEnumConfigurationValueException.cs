using System;

namespace Basalt.UniversalFileSystem.Core.Exceptions;

public class InvalidEnumConfigurationValueException<T> : InvalidConfigurationValueException where T : struct, Enum
{
    public InvalidEnumConfigurationValueException(string configurationKey, object? configurationValue, string? message = null, Exception? inner = null)
        : base(configurationKey, configurationValue?.ToString() ?? "<null>",
            message ?? $"Invalid configuration [{configurationKey}] = [{configurationValue}], valid values are [{string.Join("|", Enum.GetNames<T>())}]",
            inner)
    {
        if (configurationValue == null) throw new ArgumentNullException(nameof(configurationValue));
    }
}