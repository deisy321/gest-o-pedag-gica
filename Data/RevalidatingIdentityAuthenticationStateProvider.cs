using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace gestaopedagogica.Data
{
    public class RevalidatingIdentityAuthenticationStateProvider<TUser>
        : AuthenticationStateProvider where TUser : class
    {
      private readonly IServiceScopeFactory _scopeFactory;

        public RevalidatingIdentityAuthenticationStateProvider(
            IServiceScopeFactory scopeFactory)
        {
         _scopeFactory = scopeFactory;
        }

   public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
         try
            {
         using (var scope = _scopeFactory.CreateScope())
      {
    var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<TUser>>();
   var signInManager = scope.ServiceProvider
            .GetRequiredService<SignInManager<TUser>>();
  var httpContextAccessor = scope.ServiceProvider
                .GetRequiredService<IHttpContextAccessor>();

                 var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext?.User);

     if (user != null)
          {
       var identity = new ClaimsIdentity(httpContextAccessor.HttpContext?.User?.Claims, "Server");
        return new AuthenticationState(new ClaimsPrincipal(identity));
          }
  }
            }
            catch (Exception ex)
     {
      Console.WriteLine($"Error getting authentication state: {ex.Message}");
   }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
  }
}
