using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using Aliyun.OSS.Common.Authentication;

namespace Basalt.UniversalFileSystem.AliyunOss;

public class AliyunConfigJsonCredentialProvider : ICredentialsProvider
{
    private readonly Lazy<ICredentials> _credentials;
    
    public AliyunConfigJsonCredentialProvider(string? profile, string? configJsonPath = null)
    {
        _credentials = new(() =>
        {
            string configJsonPathFinal = configJsonPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aliyun", "config.json");

            var configJson = File.ReadAllText(configJsonPathFinal);
            JsonNode? jsonRoot = JsonNode.Parse(configJson);
            
            profile ??= jsonRoot?["current"]?.GetValue<string>();
            if (profile == null)
                throw new Exception($"Profile is not configured and can't get it from ${configJsonPathFinal}.");

            var jsonProfiles = jsonRoot?["profiles"];

            JsonNode? jsonProfile = jsonProfiles?.AsArray().FirstOrDefault(x => x?["name"]?.GetValue<string>() == profile);

            if (jsonProfile == null)
                throw new Exception($"Credential profile ${profile} is not found in file {configJsonPath}.");

            string? accessKeyId = jsonProfile["access_key_id"]?.GetValue<string>();
            string? accessKeySecret = jsonProfile["access_key_secret"]?.GetValue<string>();
            string? securityToken = jsonProfile["sts_token"]?.GetValue<string>();

            if (accessKeyId == null || accessKeySecret == null || securityToken == null)
                throw new Exception($"Cannot get access keys or token from file {configJsonPath}.");

            return new DefaultCredentials(accessKeyId, accessKeySecret, securityToken);
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }
    
    public void SetCredentials(ICredentials creds)
    {
        throw new NotImplementedException();
    }

    public ICredentials GetCredentials()
    {
        return _credentials.Value;
    }
}
