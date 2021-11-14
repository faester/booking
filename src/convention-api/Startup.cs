using convention_api.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace convention_api
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
            services.AddControllers();

            services.AddAuthorization(opts =>
            {
                //opts.DefaultPolicy = "bearer";
                Authorization.PoliciesSetup.EnablePolicies(opts);
            }).RegisterAuthorizationHandlers();
            services.AddAuthentication(
                    opt =>
                    {
                        opt.DefaultChallengeScheme = "oidc";
                        opt.DefaultAuthenticateScheme = "Bearer";
                    })
                .AddJwtBearer("oidc",
                    options =>
                    {
                        options.MetadataAddress = "https://identity-server.mfaester.dk/.well-known/openid-configuration";
                        options.Audience = "convention-api";
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
