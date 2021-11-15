using convention_website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using convention_website.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace convention_website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Policy = PolicyNames.Administration)]
        public IActionResult Administrator()
        {
            return Content("Administrator", "text/plain");
        }

        [Authorize(Policy = PolicyNames.SpeakerAccess)]
        public IActionResult Speaker()
        {
            return Content("Speaker", "text/plain");
        }

        [Authorize(Policy = PolicyNames.ValidatedUserAccess)]
        public IActionResult Private()
        {
            return Content("Logged in user", "text/plain");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
