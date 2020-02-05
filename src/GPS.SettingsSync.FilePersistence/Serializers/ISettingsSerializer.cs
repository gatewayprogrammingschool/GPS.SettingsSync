using System.Collections.Generic;
using GPS.SettingsSync.Core.Collections;

namespace GPS.SettingsSync.FilePersistence.Serializers
{
    public interface ISettingsSerializer
    {
        byte[] Serialize(IDistributedPropertySet data);
        IDistributedPropertySet Deserialize(byte[] source);
    }
}