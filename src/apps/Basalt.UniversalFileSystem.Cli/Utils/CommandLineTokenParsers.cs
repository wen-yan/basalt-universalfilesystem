using System;
using System.CommandLine.Parsing;
using System.Linq;

namespace Basalt.UniversalFileSystem.Cli.Utils;

internal static class CommandLineTokenParsers
{
    public static Uri? UriParser(SymbolResult result)
    {
        if (result.Tokens.Count == 1)
            return new Uri(result.Tokens.First().Value);
        else if (result.Tokens.Count == 0)
            return null;
        else
        {
            result.AddError("No tokens found");
            return null; // Ignored.
        }
    }
    
    public static Uri[] UrisParser(SymbolResult result)
    {
        return result.Tokens.Select(x => x.Value).Select(x => new Uri(x)).ToArray();
    }
}