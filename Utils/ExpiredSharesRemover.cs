using ShareYourCAD.Models;
using ShareYourCAD.Services;

namespace ShareYourCAD.Utils;

public class ExpiredSharesRemover
{
    public static async Task Run(StorageService _storageService,
                                 Models.Settings.FileStorageSettings _config,
                                 ILogger<StorageService> _logger)
    {
        DateTime expirationCutoff = (DateTime.Now - TimeSpan.FromSeconds(_config.MaxAgeSeconds));
        var expiredShares = await _storageService.GetExpirationCandidateShares(expirationCutoff);

        int numExpiredShares = 0;
        foreach (Share expiredShare in expiredShares)
        {
            expiredShare.Status = ShareStatus.Expired;
            await _storageService.UpdateShare(expiredShare);
            _storageService.RemoveModels(expiredShare);
            numExpiredShares += 1;
        }
        _logger.LogInformation($"Deleted {numExpiredShares} expired shares...");
    }
}
