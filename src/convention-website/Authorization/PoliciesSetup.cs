using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace convention_website.Authorization
{
    public static class PoliciesSetup
    {
        public static void EnablePolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(PolicyNames.Administration,
                policy =>
                {
                    policy.AuthenticationSchemes.Add("oidc");
                    policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
                    policy.Requirements.Add(new AdministratorRequirement());
                });
            options.AddPolicy(PolicyNames.SpeakerAccess,
                policy =>
                {
                    policy.AuthenticationSchemes.Add("oidc");
                    policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
                    policy.Requirements.Add(new SpeakerRequirement());
                });
            options.AddPolicy(PolicyNames.ValidatedUserAccess,
                policy =>
                {
                    policy.AuthenticationSchemes.Add("oidc");
                    policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
                    policy.Requirements.Add(new ValidatedUserRequirement());
                });
        }

        public static IServiceCollection RegisterAuthorizationHandlers(this IServiceCollection collection)
        {
            collection.AddSingleton<IAuthorizationHandler, AdministratorAccessHandler>();
            collection.AddSingleton<IAuthorizationHandler, ValidatedUserAccessHandler>();
            collection.AddSingleton<IAuthorizationHandler, SpeakerAccessHandler>();
            return collection;
        }
    }

    public class AdministratorRequirement : IAuthorizationRequirement
    {
    }

    public class SpeakerRequirement : IAuthorizationRequirement
    {
    }

    public class ValidatedUserRequirement : IAuthorizationRequirement
    {
    }

    public class SpeakerAccessHandler : AuthorizationHandler<SpeakerRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SpeakerRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == "role" && claim.Value == "speaker"))
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

    public class ValidatedUserAccessHandler : AuthorizationHandler<ValidatedUserRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidatedUserRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == "role" && claim.Value == "validated_user"))
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

    public class AdministratorAccessHandler : AuthorizationHandler<AdministratorRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdministratorRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == "role" && claim.Value == "administrator"))
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
        public const string SpeakerAccess = "SpeakerAccess";
        public const string ValidatedUserAccess = "ValidatedUserAccess";
    }
}
