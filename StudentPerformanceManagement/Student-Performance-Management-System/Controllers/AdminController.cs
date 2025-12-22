using Microsoft.AspNetCore.Mvc;

namespace Student_Performance_Management_System.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
