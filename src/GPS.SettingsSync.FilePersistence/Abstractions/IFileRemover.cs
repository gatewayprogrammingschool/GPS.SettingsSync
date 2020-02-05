namespace GPS.SettingsSync.FilePersistence.Abstractions
{
    public interface IFileRemover
    {
        string RemoveFile(string name, string path);
    }
}