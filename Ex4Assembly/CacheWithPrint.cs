using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ex4Assembly
{
    public sealed class CacheWithPrint<T>(TimeSpan recordsLifetime, uint cacheSize) : Cache<T>(recordsLifetime, cacheSize)
    {
        public ConcurrentDictionary<string, T> getState()
        {
            return _dict;
        }

        public void PrintDictionary()
        {
            Console.WriteLine("Dictionary state:");
            var enumerable = _dict.AsEnumerable();
            for (int i = 0; i < enumerable.Count(); ++i)
            {
                Console.WriteLine($"{enumerable.ElementAt(i).Key}={enumerable.ElementAt(i).Value}");
            }
            Console.WriteLine(); 
        }
    }
}
