using Microsoft.AspNetCore.Mvc;

namespace EasyPicPay.Web.Controllers;

public class TransactionController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}