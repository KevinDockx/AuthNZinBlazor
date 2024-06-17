using Microsoft.AspNetCore.Authorization;

namespace BlazorAuthNZDemo.AuthorizationPolicies;

public static class Policies
{
    public const string IsFromBelgium = "IsFromBelgium";

    public static AuthorizationPolicy IsFromBelgiumPolicy()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim("ctry", "BE")
            .Build();
    }
}
