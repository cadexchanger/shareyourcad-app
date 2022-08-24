using Microsoft.AspNetCore.Mvc;
using ShareYourCAD.Models;
using ShareYourCAD.Services;

namespace ShareYourCAD.Controllers;

[Route("[controller]")]
[ApiController]
public class ShareController : Controller
{
    private readonly ILogger<ShareController> _logger;
    private StorageService _storageService;

    public ShareController(ILogger<ShareController> logger,
                           StorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    [HttpPost("")]
    public async Task<IActionResult> PostShare(IFormFile modelFile,
                                               [FromServices] ConversionService conversionService)
    {
        _logger.LogDebug($"Received {modelFile.Name}...");

        Share share = new Share(modelFile.FileName);
        await _storageService.CreateShare(share);
        await _storageService.SaveOriginalModel(share, modelFile);

        conversionService.RunProcessModel(share, async isSuccess =>
            {
                if (isSuccess)
                {
                    share.Status = ShareStatus.Ready;
                }
                else
                {
                    share.Status = ShareStatus.Error;
                }
                await _storageService.UpdateShare(share);
            });

        return Ok(share);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetShare(string id)
    {
        Share? share = await _storageService.GetShare(id);
        if (share == null)
        {
            return NotFound();
        }

        return Ok(share);
    }

    [HttpGet("{id}/Wait")]
    public async Task<IActionResult> WaitForShare(string id,
                                                  [FromServices] ConversionService conversionService)
    {
        await conversionService.WaitForProcessingTask(id);
        return Ok(await _storageService.GetShare(id));
    }

    [HttpGet("{id}/Model/{component}")]
    public async Task<IActionResult> GetProcessedModelComponent(string id, string component)
    {
        Share? share = await _storageService.GetShare(id);
        if (share == null)
        {
            return NotFound("Share not found");
        }
        else if (share.Status != ShareStatus.Ready)
        {
            return NotFound("Processed model not found. Share is not ready; check share status.");
        }

        Stream? stream = _storageService.GetProcessedModelComponent(share, component);
        if (stream == null)
        {
            _logger.LogError($"Missing processed model for ready share {id}");
            return NotFound();
        }

        return File(stream, "application/octet-stream");
    }

    [HttpGet("{id}/Original")]
    public async Task<IActionResult> GetOriginalModel(string id)
    {
        Share? share = await _storageService.GetShare(id);
        if (share == null)
        {
            return NotFound("Share not found");
        }

        Stream? stream = _storageService.GetOriginalModel(share);
        if (stream == null) {
            _logger.LogError($"Missing original model for share {id}");
            return NotFound();
        }

        return File(stream, "application/octet-stream");
    }
}
