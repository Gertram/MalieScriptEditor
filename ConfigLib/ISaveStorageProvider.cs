using System;

namespace ConfigLib
{
    public interface ISaveStorageProvider
    {
        void Save<T>(T config) where T : ICloneable;
    }
}
