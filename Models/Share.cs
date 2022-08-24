using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShareYourCAD.Models;

public enum ShareStatus {
    Submitted,
    InProcessing,
    Error,
    Ready,
    Expired
}

public class Share
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public ShareStatus Status { get; set; }
    public string FileName { get; set; }
    public DateTime CreatedAt { get; set; }

    public Share(string fileName)
    {
        FileName = fileName;
        Status = ShareStatus.Submitted;
        CreatedAt = DateTime.Now;
    }
}
