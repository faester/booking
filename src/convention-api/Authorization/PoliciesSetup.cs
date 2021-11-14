using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace convention_api.Authorization
{
    public static class PoliciesSetup
    {
        public static void EnablePolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(PolicyNames.Administration,
                policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.Requirements.Add(new AdministratorRequirement());
                });

        }

        public static IServiceCollection RegisterAuthorizationHandlers(this IServiceCollection collection)
        {
            collection.AddSingleton<IAuthorizationHandler, AdministratorAccessHandler>();
            return collection;
        }
    }

    public class AdministratorRequirement : IAuthorizationRequirement
    {
    }

    public class AdministratorAccessHandler : AuthorizationHandler<AdministratorRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdministratorRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == "scope" && claim.Value == "convention-admin"))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }

    
    public static class PolicyNames
    {
        public const string Administration = "Administration";
        public const string ReadAccess = "ReadAccess";
        public const string WriteOwnData = "WriteOwnData";
    }
}
