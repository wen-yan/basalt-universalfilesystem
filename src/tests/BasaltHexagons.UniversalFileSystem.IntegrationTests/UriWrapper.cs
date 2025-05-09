namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public class UriWrapper
{
    public UriWrapper(string name, string baseUri)
    {
        this.Name = name;
        this.BaseUri = new(baseUri);
    }

    public string Name { get; }
    public Uri BaseUri { get; }
    public Uri GetFullUri(string path) => new(this.BaseUri, new Uri(path, UriKind.RelativeOrAbsolute));
    public override string ToString() => this.Name;

    public static implicit operator Func<string, Uri>(UriWrapper uriWrapper) => uriWrapper.GetFullUri;
}