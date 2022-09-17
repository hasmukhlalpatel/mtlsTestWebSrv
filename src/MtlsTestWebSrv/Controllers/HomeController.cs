using Microsoft.AspNetCore.Mvc;

namespace MtlsTestWebSrv.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("test");
        }
    }
}
