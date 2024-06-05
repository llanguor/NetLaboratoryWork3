using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Ex4Assembly
{
    public class Cache<T>
    {
        private TimeSpan RecordsLifetime { get; set; }
        private uint CacheSize { get; set; }

        private protected ConcurrentDictionary<string, T> _dict = new();
        private ConcurrentQueue<string> _keysQueue = new ConcurrentQueue<string>();


        public Cache(TimeSpan recordsLifetime, uint cacheSize)
        { 
            RecordsLifetime = recordsLifetime;
            CacheSize = cacheSize;
        }

        public void Save(string key, T value)
        {
            if (_dict.ContainsKey(key))
            {
                throw new ArgumentException($"The given key '{key}' already exists in the dictionary");
            }

            if (_dict.Count == CacheSize)
            {
                string? result;
                _keysQueue.TryDequeue(out result);
                _dict.TryRemove(result!, out _);
            }

            _dict.TryAdd(key, value);
            _keysQueue.Enqueue(key);

            StartRecordTimer(RecordsLifetime, key);
        }

        public T Get(string key)
        {
            return _dict[key];
        }

        async private void StartRecordTimer(TimeSpan time, string key)
        { 
            await Task.Delay((int)time.TotalMilliseconds);
            if (_dict.TryRemove(key, out _))
            {
                _keysQueue.TryDequeue(out _);
            }
        }
    }
}
