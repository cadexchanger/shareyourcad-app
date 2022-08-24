using ShareYourCAD.Models;
using ShareYourCAD.Models.Settings;
using ShareYourCAD.Utils;
using Fluent = FluentScheduler;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace ShareYourCAD.Services;

public class StorageService
{
    private readonly IMongoCollection<Share> _sharesCollection;
    private readonly string _fileStorageLocation;

    public StorageService(IOptions<SharesDatabaseSettings> databaseSettings,
                          IOptions<FileStorageSettings> fileStorageSettings,
                          ILogger<StorageService> logger)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);

        _sharesCollection = mongoDatabase.GetCollection<Share>(
            databaseSettings.Value.CollectionName);

        _fileStorageLocation = fileStorageSettings.Value.Location;

        // Schedule file cleanup
        Fluent.JobManager.Initialize();
        Fluent.JobManager.AddJob(
            async () =>
            {
                await ExpiredSharesRemover.Run(this, fileStorageSettings.Value, logger);
            },
            s => s.ToRunEvery(fileStorageSettings.Value.PurgePeriodSeconds).Seconds());
    }

#region database-access
    public async Task CreateShare(Share share)
    {
        await _sharesCollection.InsertOneAsync(share);
    }

    public async Task UpdateShare(Share share)
    {
        await _sharesCollection.ReplaceOneAsync(s => s.Id == share.Id, share);
    }

    public async Task<Share?> GetShare(string id)
    {
        ObjectId mongoId;
        if (!MongoDB.Bson.ObjectId.TryParse(id, out mongoId))
        {
            return null;
        }

        Share? share = await _sharesCollection.Find(s => id == s.Id).FirstOrDefaultAsync();
        return share;
    }

    public async Task<IEnumerable<Share>> GetExpirationCandidateShares(DateTime expirationCutoff)
    {
        using (var cursor = await _sharesCollection.FindAsync(
            s => s.Status != ShareStatus.Expired && s.CreatedAt < expirationCutoff))
        {
            return await cursor.ToListAsync();
        }
    }

#endregion

#region filesystem-access

    public Stream? GetProcessedModelComponent(Share share, string component)
    {
        string componentPath = Path.Combine(GetProcessedModelStoragePath(share), component);

        if (File.Exists(componentPath))
        {
            return new FileStream(componentPath, FileMode.Open);
        }
        else
        {
            return null;
        }
    }

    public Stream? GetOriginalModel(Share share)
    {
        string filePath = GetOriginalModelStoragePath(share);

        if (File.Exists(filePath))
        {
            return new FileStream(filePath, FileMode.Open);
        }
        else
        {
            return null;
        }
    }

    public async Task SaveOriginalModel(Share share, IFormFile modelFile)
    {
        if (!Directory.Exists(GetModelFileStoragePrefix(share)))
        {
            Directory.CreateDirectory(GetModelFileStoragePrefix(share));
        }

        string originalPath = GetOriginalModelStoragePath(share);
        using (var originalFile = new FileStream(originalPath, FileMode.Create))
        {
            await modelFile.CopyToAsync(originalFile);
        }
    }

    public void RemoveModels(Share share)
    {
        Directory.Delete(GetModelFileStoragePrefix(share), recursive: true);
    }

    public string GetOriginalModelStoragePath(Share share)
    {
        return Path.Combine(GetModelFileStoragePrefix(share),
                            $"original{Path.GetExtension(share.FileName)}");
    }

    public string GetProcessedModelStoragePath(Share share)
    {
        return Path.Combine(GetModelFileStoragePrefix(share),
                            "processed");
    }

    private string GetModelFileStoragePrefix(Share share)
    {
        string filesFolder = Path.Combine(_fileStorageLocation, share.Id!);
        return filesFolder;
    }

#endregion

}
