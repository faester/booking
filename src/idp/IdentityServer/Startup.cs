﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon.SimpleDB;
using Amazon.SimpleSystemsManagement;
using ConventionApiLibrary;
using IdentityServer.DataAccess;
using IdentityServer.DataProtection;
using IdentityServer.Quickstart;
using IdentityServer.Quickstart.Account;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
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

            services.AddDataProtection(options =>
            {
                options.ApplicationDiscriminator = "idp";
            });

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
            services.AddScoped<SimpleDbUserStore>();
            services.AddScoped<ISimpleDbDomainName>(ctxt => new SimpleDbDomainName("users"));
            services.AddScoped<SimpleDbBasedStore<SimpleDbUserStore.TestUserDto>>();
            services.AddScoped<ISimpleDbConverter<SimpleDbUserStore.TestUserDto>, TestUserConverter>();
            services.AddScoped<ISimpleDbConverter<UserInformation>, UserInfoConverter>();
            services.AddScoped<IPasswordFunctions, BCryptPasswordFunctions>();
            services.AddScoped<IPersistedGrantStore, SimpleDbPersistedGrantStore>();
            services.AddScoped<IAmazonSimpleDB, AmazonSimpleDBClient>(service => new AmazonSimpleDBClient(SecretsRetriever.GetCredentials(), SecretsRetriever.Region));

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

    public class SimpleDbPersistedGrantStore : IPersistedGrantStore
    {
        public Task StoreAsync(PersistedGrant grant)
        {
            throw new NotImplementedException();
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}
