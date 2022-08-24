using ShareYourCAD.Models;
using ShareYourCAD.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ShareYourCAD.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private StorageService _storageService;

    public HomeController(ILogger<HomeController> logger,
                          StorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    public IActionResult Index()
    {
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

    public async Task<IActionResult> Share(string? id)
    {
        Share? share = await _storageService.GetShare(id!);

        if (share != null)
        {
            return View(share);
        }

        return NotFound("The requested shared model does not exist.");
    }
}
