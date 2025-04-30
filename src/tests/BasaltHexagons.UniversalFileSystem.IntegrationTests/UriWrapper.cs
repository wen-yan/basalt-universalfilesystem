namespace BasaltHexagons.UniversalFileSystem.IntegrationTests;

public class UriWrapper
{
    public UriWrapper(string baseUri)
    {
        this.BaseUri = new(baseUri);
    }

    public Uri BaseUri { get; }
    public Uri Apply(string path) => new(this.BaseUri, new Uri(path, UriKind.RelativeOrAbsolute));
    public override string ToString() => this.BaseUri.Scheme;

    public static implicit operator Func<string, Uri>(UriWrapper uriWrapper) => uriWrapper.Apply;
}