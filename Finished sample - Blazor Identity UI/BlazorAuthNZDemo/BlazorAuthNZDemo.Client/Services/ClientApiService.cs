using BlazorAuthNZDemo.Client.Models;
using System.Text.Json;

namespace BlazorAuthNZDemo.Client.Services;

public class ClientApiService([FromKeyedServices("LocalAPIClientFromWASM")] HttpClient localAPIClient,
    [FromKeyedServices("RemoteAPIClientFromWASM")] HttpClient remoteAPIClient) : IApiService
{
    private readonly HttpClient _localAPIClient = localAPIClient;
    private readonly HttpClient _remoteAPIClient = remoteAPIClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new ()
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<IEnumerable<Band>> CallLocalApiAsync()
    {     
        var response = await _localAPIClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get,
                "localapi/bands"));
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<Band>>(
            await response.Content.ReadAsStreamAsync(),
            _jsonSerializerOptions,
            CancellationToken.None) ?? [];
    }

    public async Task<IEnumerable<Band>> CallRemoteApiAsync()
    { 
        var response = await _remoteAPIClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get,
                "remoteapi/bands"));
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<Band>>(
           await response.Content.ReadAsStreamAsync(),
           _jsonSerializerOptions,
           CancellationToken.None) ?? [];
    }
}
