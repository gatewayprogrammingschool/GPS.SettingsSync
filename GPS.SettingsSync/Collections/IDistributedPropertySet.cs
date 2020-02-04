using System.Collections.Generic;

namespace GPS.SettingsSync.Collections
{
    public interface IDistributedPropertySet : IEnumerable<KeyValuePair<string, object>>, IDictionary<string, object>,
        IObservableMap<string, object>
    {

    }
}