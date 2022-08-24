namespace ShareYourCAD.Models.Settings;

public class FileStorageSettings
{
    public string Location { get; set; } = null!;
    public int MaxAgeSeconds { get; set; } = 0;
    public int PurgePeriodSeconds { get; set; } = 0;
}
