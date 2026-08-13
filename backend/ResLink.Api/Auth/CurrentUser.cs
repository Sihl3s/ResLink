using System.Security.Claims;

namespace ResLink.Api.Auth;

public class CurrentUser(IHttpContextAccessor accessor)
{
    public Guid UserId =>
        Guid.Parse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("User is not authenticated."));

    public string Role =>
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role)
        ?? accessor.HttpContext?.User.FindFirstValue("role")
        ?? throw new InvalidOperationException("User role is missing.");

    public Guid? ResidenceId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue("residenceId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
