using BlazorAuthNZDemo.AuthorizationPolicies;
using BlazorAuthNZDemo.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Text.Json;

namespace BlazorAuthNZDemo.Client.Services;

public class ClientApiService([FromKeyedServices("LocalAPIClientFromWASM")] HttpClient localAPIClient,
    NavigationManager navigationManager, 
    IAuthorizationService authorizationService, 
    AuthenticationStateProvider authenticationStateProvider) : IApiService
{
    private readonly HttpClient _localAPIClient = localAPIClient;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new ()
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<IEnumerable<Band>> CallLocalApiAsync()
    {
        var authenticationState = 
            await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (!((await _authorizationService.AuthorizeAsync(authenticationState.User,
            Policies.IsFromBelgiumPolicy())).Succeeded))
        {
            _navigationManager.NavigateTo("AccessDenied");
            return [];
        }

        var request = new HttpRequestMessage(HttpMethod.Get,
            "localapi/bands");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var response = await _localAPIClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<Band>>(
            await response.Content.ReadAsStreamAsync(),
            _jsonSerializerOptions,
            CancellationToken.None) ?? [];

    }

    public async Task<IEnumerable<Band>> CallRemoteApiAsync()
    {
        var authenticationState = await _authenticationStateProvider
            .GetAuthenticationStateAsync();
        if (!((await _authorizationService.AuthorizeAsync(
                authenticationState.User,
                Policies.IsFromBelgiumPolicy())).Succeeded))
        {
            _navigationManager.NavigateTo("AccessDenied");
            return [];
        }

        var request = new HttpRequestMessage(HttpMethod.Get,
           "forward-to-remote-api/bands");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var response = await _localAPIClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<List<Band>>(
           await response.Content.ReadAsStreamAsync(),
           _jsonSerializerOptions,
           CancellationToken.None) ?? [];
    }
}
