using Microsoft.AspNetCore.Authorization;

namespace ArtemisBankingPro.WebApp.Filters;

public class RoleAuthorizeAttribute : AuthorizeAttribute
{
    public RoleAuthorizeAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}