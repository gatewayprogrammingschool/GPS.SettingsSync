using GPS.SettingsSync.Core;
using GPS.SettingsSync.FilePersistence.Abstractions;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public class BinaryPersistenceProvider : PersistenceProviderBase
    {
        public override FileTypes FileType => FileTypes.Binary;

        public BinaryPersistenceProvider(
            BinaryFileReader reader
            , BinaryFileWriter writer
            , IFileRemover remover
            , ILogger<BinaryPersistenceProvider> logger
            , ISettingsMetadata metadata) : base(logger, reader, writer, remover, metadata)
        {
        }
    }
}