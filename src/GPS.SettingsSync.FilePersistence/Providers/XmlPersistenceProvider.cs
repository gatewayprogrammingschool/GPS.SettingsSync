using GPS.SettingsSync.Core;
using GPS.SettingsSync.FilePersistence.Abstractions;
using Microsoft.Extensions.Logging;

namespace GPS.SettingsSync.FilePersistence.Providers
{
    public class XmlPersistenceProvider : PersistenceProviderBase
    {
        public override FileTypes FileType => FileTypes.XML;

        public XmlPersistenceProvider(
            XmlFileReader reader
            , XmlFileWriter writer
            , IFileRemover remover
            , ILogger<XmlPersistenceProvider> logger
            , ISettingsMetadata metadata) : base(logger, reader, writer, remover, metadata)
        {
        }
    }

}