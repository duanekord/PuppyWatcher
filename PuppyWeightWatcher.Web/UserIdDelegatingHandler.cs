using System.Security.Claims;

namespace PuppyWeightWatcher.Web;

public class UserIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.FindFirst(ClaimTypes.Name)?.Value
                ?? "";
            request.Headers.Remove("X-User-Id");
            request.Headers.Add("X-User-Id", email);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
