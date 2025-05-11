using System.Collections.Concurrent;

namespace ImageProcessingApi.Services
{
    public interface ICacheService
    {
        byte[] Get(string key);
        void Set(string key, byte[] data);
    }

    public class CacheService : ICacheService
    {
        // Creates a thread-safe dictionary to store cached items,
        // where the key is a string (e.g., image hash + filter) and the value is the processed image (as a byte array).
        private readonly ConcurrentDictionary<string, byte[]> _cache = new();

        // Attempts to retrieve a cached item by key.
        // If the key exists, returns the byte array; otherwise, returns null.
        public byte[] Get(string key)
        {
            _cache.TryGetValue(key, out byte[] cachedData);
            return cachedData;
        }

        // Stores or updates a cached item in the dictionary with the given key and byte array value.
        public void Set(string key, byte[] data)
        {
            _cache[key] = data;
        }
    }
}