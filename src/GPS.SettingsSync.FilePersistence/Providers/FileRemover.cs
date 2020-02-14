using System.IO;
using GPS.SettingsSync.FilePersistence.Abstractions;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public class FileRemover : IFileRemover
    {
        public string RemoveFile(string name, string path)
        {
            var fullPath = Path.Combine(path, name);
            if(File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return fullPath;
            }
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath);
                return fullPath;
            }

            return null;
        }
    }
}