using BlazorAuthNZDemo.AuthorizationPolicies;
using BlazorAuthNZDemo.Client.Models;
using BlazorAuthNZDemo.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;

namespace BlazorAuthNZDemo.Services;

public class ServerApiService(IHttpClientFactory httpClientFactory,
    BandsRepository bandsRespository, 
    IAuthorizationService authorizationService, 
    AuthenticationStateProvider authenticationStateProvider, 
    NavigationManager navigationManager) : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly BandsRepository _bandsRepository = bandsRespository;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<IEnumerable<Band>> CallLocalApiAsync()
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (!((await _authorizationService.AuthorizeAsync(
                authenticationState.User,
                Policies.IsFromBelgiumPolicy())).Succeeded))
        {
            _navigationManager.NavigateTo("AccessDenied");
            return [];
        }

        return await Task.FromResult(_bandsRepository.GetBands());
    }

    public async Task<IEnumerable<Band>> CallRemoteApiAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("RemoteAPIClient");

        var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                    "remoteapi/bands");
        var response = await httpClient.SendAsync(requestMessage);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden
            || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _navigationManager.NavigateTo("AccessDenied");
            return [];
        }
        else
        {
            response.EnsureSuccessStatusCode();

            return await JsonSerializer.DeserializeAsync<List<Band>>(
               await response.Content.ReadAsStreamAsync(),
               _jsonSerializerOptions,
               CancellationToken.None) ?? [];
        }

    }
}
