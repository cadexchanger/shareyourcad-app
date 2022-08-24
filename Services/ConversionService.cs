using System.Collections.Concurrent;
using ShareYourCAD.Models;
using cadex;

namespace ShareYourCAD.Services;

public class ConversionService
{
    private readonly ILogger<ConversionService> _logger;
    private StorageService _storageService;
    private ConcurrentDictionary<string, Task> _processingTasks = new ConcurrentDictionary<string, Task>();

    public ConversionService(ILogger<ConversionService> logger,
                             StorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;

        string key = LicenseKey.Value();
        if (!LicenseManager.Activate(key))
        {
            throw new ApplicationException("Failed to activate CAD Exchanger license");
        }
    }

    public void RunProcessModel(Share share, Action<bool> postProcess)
    {
        Task processingTask = ProcessModel(share).ContinueWith(t => postProcess(t.Result));
        _processingTasks.TryAdd(share.Id!, processingTask);
    }

    public async Task WaitForProcessingTask(string id)
    {
        Task? task;
        if (_processingTasks.TryRemove(id, out task))
        {
            await task;
        }
    }

    private async Task<bool> ProcessModel(Share share)
    {
        share.Status = ShareStatus.InProcessing;
        await _storageService.UpdateShare(share);

        ModelData_ModelReader reader = new ModelData_ModelReader();
        ModelData_Model model = new ModelData_Model();
        string originalPath = _storageService.GetOriginalModelStoragePath(share);
        bool result = reader.Read(new Base_UTF16String(originalPath), model);

        if (!result)
        {
            _logger.LogError($"Import of model {share.Id} failed");
            return false;
        }

        ModelData_ModelWriter writer = new ModelData_ModelWriter();
        ModelData_WriterParameters param = new ModelData_WriterParameters();
        param.SetFileFormat(ModelData_WriterParameters.FileFormatType.Cdxfb);
        writer.SetWriterParameters(param);

        string cdxfbDirectory = _storageService.GetProcessedModelStoragePath(share);
        string cdxfbFile = Path.Combine(cdxfbDirectory, "scenegraph.cdxfb");
        result = writer.Write(model, new Base_UTF16String(cdxfbFile));

        if (!result)
        {
            _logger.LogError($"CDXFB export of model {share.Id} failed");
            return false;
        }

        return true;
    }
}
