// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Security.Cryptography;
using Amazon.SimpleDB;
using IdentityServer.Quickstart;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            var builder = services.AddIdentityServer(options =>
            {
                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients);

            builder.AddSigningCredential(CreateSigningCredentials());
            services.AddScoped<IUserStore, SimpleDbUserStore>();
            services.AddSingleton<IPasswordFunctions, BCryptPasswordFunctions>();
            services.AddScoped<IAmazonSimpleDB, AmazonSimpleDBClient>(service => new AmazonSimpleDBClient(SecretsRetriever.GetCredentials(), SecretsRetriever.Region));
        }

        private SigningCredentials CreateSigningCredentials()
        {
            RSAParameters rsaParameters = SecretsRetriever.GetRSAParameters("secrets/main_rsa_key");
            SecurityKey key = new RsaSecurityKey(rsaParameters);
            return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            var forwardHeaderSettings = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto,
            };
            forwardHeaderSettings.KnownNetworks.Clear();
            forwardHeaderSettings.AllowedHosts.Clear();
            app.UseForwardedHeaders(forwardHeaderSettings);
            // uncomment if you want to add MVC
            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
