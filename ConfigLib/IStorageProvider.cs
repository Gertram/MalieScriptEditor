using System;

namespace ConfigLib
{
    public interface IStorageProvider : ISaveStorageProvider
    {
        T Get<T>() where T : class, ICloneable, new();
        IStorageProvider GetSection(string name);

    }
}
