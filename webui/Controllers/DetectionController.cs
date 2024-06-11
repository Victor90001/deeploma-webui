using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using webui.Models;

namespace webui.Controllers
{
    public class DetectionController : Controller
    {
        private readonly ILogger<DetectionController> _logger;

        public DetectionController(ILogger<DetectionController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Detection()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
