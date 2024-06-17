using BlazorAuthNZDemo.Client.Models;

namespace BlazorAuthNZDemo.Client.Services;

public interface IApiService
{
    Task<IEnumerable<Band>> CallLocalApiAsync();

    Task<IEnumerable<Band>> CallRemoteApiAsync();

}
