using System;
using GPS.SettingsSync.Core;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.FilePersistence
{
    public class FilePersistenceMapping
    {
        public DistributedApplicationDataContainer Container { get; }
        public SettingsScopes SettingsScope { get; }
        public string FileName { get; }
        public string FilePath { get; }

        public FilePersistenceMapping(DistributedApplicationDataContainer container
            , SettingsScopes settingsScope
            , string fileName, string filePath)
        {
            Container = container ?? throw new ArgumentNullException(nameof(container));
            SettingsScope = settingsScope;
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            Container.Values.MapChanged += ValuesOnMapChanged;
        }

        private void ValuesOnMapChanged(IObservableMap<string, object> sender, IMapChangedEventArgs<string> args)
        {
            switch (args.DistributedCollectionChange)
            {
                case DistributedCollectionChange.Reset:
                    FilePersistenceManager.Current.ResetFile(this);
                    break;

                case DistributedCollectionChange.ItemRemoved:
                case DistributedCollectionChange.ItemInserted:
                case DistributedCollectionChange.ItemChanged:
                    FilePersistenceManager.Current.UpdateFile(this);
                    break;

                default:
                    break;
            }
        }
    }
}