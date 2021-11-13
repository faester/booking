// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IdentityServerHost.Quickstart.UI
{
    public class UserInformation
    {
        [Required]
        [RegularExpression(pattern: @"[a-z0-9_.-]+@[a-z0-9_.-]+\.[a-z]{2,}", ErrorMessage = "Email does not look like an e-mail")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(pattern: @"\+[0-9 ]+", ErrorMessage = "Phone is not valid. Numbers and spaces only. ")]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

    }

    public class SignupViewModel : UserInformation
    {
        [Required]
        [RegularExpression(pattern : "[a-z0-9_-]{3,}", ErrorMessage = "Username must be at least 3 characters and contain alphanumerics, dashes and underscores")]
        public string Username { get; set; }
        [Required]
        [RegularExpression(pattern : "(.){6,}", ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
        public bool TermsAccepted { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class LoginViewModel : LoginInputModel
    {
        public bool AllowRememberLogin { get; set; } = true;
        public bool EnableLocalLogin { get; set; } = true;

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
        public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));

        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;
    }
}