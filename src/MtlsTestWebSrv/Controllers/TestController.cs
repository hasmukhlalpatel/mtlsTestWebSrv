using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MtlsTestWebSrv.Controllers;

//[Authorize]
public class TestController : Controller
{
    [Route("/ProvWeb/ezapi/CollectionsAPI/request")]
    public IActionResult Index()
    {
        return Content("test request");
    }
}