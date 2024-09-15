using Microsoft.AspNetCore.Mvc;

namespace Producer_API.Controllers
{
    public class PostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
