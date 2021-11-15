using convention_website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using convention_website.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace convention_website.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
