using Microsoft.AspNetCore.Mvc;

public class ProjectsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}