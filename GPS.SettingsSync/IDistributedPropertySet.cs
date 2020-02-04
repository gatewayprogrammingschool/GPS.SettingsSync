using System.Collections.Generic;

namespace GPS.SettingsSync
{
    public interface IDistributedPropertySet : IEnumerable<KeyValuePair<string, object>>, IDictionary<string, object>,
        IObservableMap<string, object>
    {

    }
}