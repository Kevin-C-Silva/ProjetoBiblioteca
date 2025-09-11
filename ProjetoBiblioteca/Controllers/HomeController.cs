using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjetoBiblioteca.Models;
using ProjetoBiblioteca.Autenticacao;

namespace ProjetoBiblioteca.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (!HttpContext.Session.GetInt32(SessionKeys.UserId).HasValue)
            return RedirectToAction("Login", "Auth");

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
