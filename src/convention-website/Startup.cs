using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.JsonWebTokens;
using convention_website.Authorization;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;


namespace convention_website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opts.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = "oidc";
                }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect("oidc", opts =>
                {
                    opts.MetadataAddress = "https://identity-server.mfaester.dk/.well-known/openid-configuration";
                    opts.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
                    opts.ClientId = "convention-website";
                    opts.Scope.Clear();
                    opts.Scope.Add(JwtRegisteredClaimNames.Email);
                    opts.Scope.Add("phone");
                    opts.Scope.Add("role");
                    opts.Scope.Add("openid");
                    opts.UsePkce = true;
                    opts.ClientSecret = null;
                    opts.GetClaimsFromUserInfoEndpoint = true;
                    opts.ResponseType = "code id_token";
                    opts.CallbackPath = "/signin-oidc";
                    opts.ClaimActions.Add(new MapAllClaimsAction());
                });

            services.AddAuthorization(opts =>
            {
                opts.EnablePolicies();
                ;
            }).RegisterAuthorizationHandlers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var forwardHeaderSettings = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto,
            };
            forwardHeaderSettings.KnownNetworks.Clear();
            forwardHeaderSettings.AllowedHosts.Clear();
            app.UseForwardedHeaders(forwardHeaderSettings);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
