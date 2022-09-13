using Microsoft.AspNetCore.Mvc;

namespace adworks.media_web_api.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}