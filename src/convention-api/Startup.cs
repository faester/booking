using System;
using convention_api.Authorization;
using convention_api.Clients.BreweryClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

            services.AddAuthentication(opts =>
            {
                opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
            options =>
                         {
                             options.MetadataAddress = "https://identity-server.mfaester.dk/.well-known/openid-configuration";
                             options.Audience = "https://identity-server.mfaester.dk/resources";
                         });
            services.RegisterAuthorizationHandlers();
            services.AddAuthorization(opts =>
            {
                opts.EnablePolicies();
            });
            services.AddSingleton<IBreweryClient>(ctxt => new CachingBreweryClient(new BreweryClient(new Uri("https://api.openbrewerydb.org/")), TimeSpan.FromHours(1)));
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
