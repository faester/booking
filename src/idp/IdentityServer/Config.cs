// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Address(),
                new IdentityResources.Phone(),
                new IdentityResources.Email(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("convention-admin", "Administrate conventions"),
                new ApiScope("convention-read", "Read convention data"),
            };

        public static IEnumerable<Client> Clients =>
            new[]
            {
                new Client {
                    ClientId = "convention-reader",
                    ClientName = "convention-reader",
                    ClientSecrets = new Secret[]{new Secret("secret01", "This secret is not that secret", null)},
                    AllowedScopes = { "convention-admin", "convention-read" },
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowAccessTokensViaBrowser = true,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AccessTokenLifetime = 3600,
                    AuthorizationCodeLifetime = 300,
                }
            };
    }
}
