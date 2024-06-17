using BlazorAuthNZDemo.Client.Models;
using BlazorAuthNZDemo.Client.Services;
using System.Text.Json;

namespace BlazorAuthNZDemo.Services;

public class ServerApiService(IHttpClientFactory httpClientFactory) : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<IEnumerable<Band>> CallLocalApiAsync()
    {
        // no additional security needed: the cookie protects the host, including local APIs 
        var httpClient = _httpClientFactory.CreateClient("LocalAPIClient");

        var response = await httpClient.SendAsync(
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
        var httpClient = _httpClientFactory.CreateClient("RemoteAPIClient");

        var response = await httpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get,
                "remoteapi/bands"));
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<Band>>(
           await response.Content.ReadAsStreamAsync(),
           _jsonSerializerOptions,
           CancellationToken.None) ?? [];
    }
}
