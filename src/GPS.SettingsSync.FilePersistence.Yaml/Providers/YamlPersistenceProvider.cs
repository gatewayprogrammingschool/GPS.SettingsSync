using GPS.SettingsSync.Core;
using GPS.SettingsSync.FilePersistence.Abstractions;
using GPS.SettingsSync.FilePersistence.Providers;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Yaml.Providers
{
    public class YamlPersistenceProvider : PersistenceProviderBase
    {
        public override FileTypes FileType => FileTypes.Other;

        public override string FileExtension => "Yaml";

        public YamlPersistenceProvider(
            YamlFileReader reader
            , YamlFileWriter writer
            , IFileRemover remover
            , ILogger<YamlPersistenceProvider> logger
            , ISettingsMetadata metadata) : base(logger, reader, writer, remover, metadata)
        {
        }
    }
}