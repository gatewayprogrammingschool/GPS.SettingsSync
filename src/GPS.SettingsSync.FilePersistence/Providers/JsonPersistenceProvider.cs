using GPS.SettingsSync.Core;
using GPS.SettingsSync.FilePersistence.Abstractions;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public class JsonPersistenceProvider : PersistenceProviderBase
    {
        public override FileTypes FileType => FileTypes.JSON;

        public JsonPersistenceProvider(
            JsonFileReader reader
            , JsonFileWriter writer
            , IFileRemover remover
            , ILogger<JsonPersistenceProvider> logger
            , ISettingsMetadata metadata) : base(logger, reader, writer, remover, metadata)
        {
        }
    }
}