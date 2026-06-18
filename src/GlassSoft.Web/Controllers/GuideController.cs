using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class GuideController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
