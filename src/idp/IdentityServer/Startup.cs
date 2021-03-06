// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Security.Cryptography;
using Amazon.SimpleDB;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using ConventionApiLibrary.DataAccess;
using IdentityServer.DataAccess;
using IdentityServer.DataProtection;
using IdentityServer.Quickstart;
using IdentityServer.Quickstart.Account;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
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
            services.AddScoped<SimpleDbBasedStore<UserInformation>>();
            services.AddScoped<SimpleDbBasedStore<PersistedGrantDto>>();
            services.AddScoped<ISimpleDbConverter<PersistedGrantDto>, PersistedGrantDtoConverter>();
            services.AddScoped<SimpleDbUserStore>();
            services.AddScoped<ISimpleDbDomainName<SimpleDbUserStore.TestUserDto>>(ctxt => new SimpleDbDomainName<SimpleDbUserStore.TestUserDto>("users"));
            services.AddScoped<ISimpleDbDomainName<UserInformation>>(ctxt => new SimpleDbDomainName<UserInformation>("userinformation"));
            services.AddScoped<ISimpleDbDomainName<PersistedGrantDto>>(ctxt => new SimpleDbDomainName<PersistedGrantDto>("grants"));
            services.AddScoped<SimpleDbBasedStore<SimpleDbUserStore.TestUserDto>>();
            services.AddScoped<ISimpleDbConverter<SimpleDbUserStore.TestUserDto>, TestUserConverter>();
            services.AddScoped<ISimpleDbConverter<UserInformation>, UserInfoConverter>();
            services.AddScoped<IPasswordFunctions, BCryptPasswordFunctions>();
            services.AddScoped<IPersistedGrantStore, SimpleDbPersistedGrantStore>();
            services.AddScoped<IAmazonSimpleDB, AmazonSimpleDBClient>(service => new AmazonSimpleDBClient(SecretsRetriever.GetCredentials(), SecretsRetriever.Region));
            services.AddScoped<IEventService, SqsEventService>(ctxt =>
                    new SqsEventService(new AmazonSQSClient(SecretsRetriever.GetCredentials(), SecretsRetriever.Region),
                new Uri("https://sqs.eu-west-1.amazonaws.com/539839626842/booking-audit-events")));
            services.AddSingleton<IXmlRepository, SsmDataprotection>(sers => new SsmDataprotection("/idp/ixmlrepository/", CreateSsmClient()));
        }

        private IAmazonSimpleSystemsManagement CreateSsmClient()
        {
            return new AmazonSimpleSystemsManagementClient(SecretsRetriever.GetCredentials());
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
