using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace GPS.SettingsSync.Core.Collections
{
    public interface IDistributedPropertySet : 
        IObservableMap<string, object>
        , ISerializable
        , IXmlSerializable
    {
        IReadOnlyDictionary<string, object> AsReadOnlyDictionary();
        IDistributedPropertySet SetValues(IDictionary<string, object> data);
    }
}