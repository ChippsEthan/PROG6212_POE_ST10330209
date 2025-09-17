using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Prog6212POE2B.Models;

namespace Prog6212POE2B.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Home Page
        public IActionResult Index()
        {
            return View();
        }

        // Privacy Page
        public IActionResult Privacy()
        {
            return View();
        }

        // Login Page
        public IActionResult Login()
        {
            return View();
        }

        // Lecturer Dashboard
        public IActionResult LecturerDashboard()
        {
            return View();
        }

        // Manager Dashboard
        public IActionResult ManagerDashboard()
        {
            return View();
        }

        // Error Page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
