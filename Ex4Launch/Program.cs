using Ex4Assembly;

namespace Ex4Launch
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cacheObject = new CacheWithPrint<int>(
                recordsLifetime: new TimeSpan(0, 0, 3),
                cacheSize: 4);

            cacheObject.Save("VAL1", 1);
            cacheObject.PrintDictionary();
            Thread.Sleep(500);

            Console.WriteLine($"Result from Get(VAL1):{cacheObject.Get("VAL1")}\n");

            cacheObject.Save("VAL2", 2);
            cacheObject.PrintDictionary();
            Thread.Sleep(500);

            cacheObject.Save("VAL3", 3);
            cacheObject.PrintDictionary();
            Thread.Sleep(500);

            cacheObject.Save("VAL4", 3);
            cacheObject.PrintDictionary();
            Thread.Sleep(500);

            cacheObject.Save("VAL5", 3);
            cacheObject.PrintDictionary();
            Thread.Sleep(500);

            cacheObject.Save("VAL6", 3);
            cacheObject.PrintDictionary();
            Thread.Sleep(500);

            Console.WriteLine($"Result from Get(VAL3):{cacheObject.Get("VAL3")}");

            try
            {
                Console.Write($"Result from Save(VAL5): ");
                cacheObject.Save("VAL5", 1);
                cacheObject.PrintDictionary();
                Thread.Sleep(500);
            }
            catch(ArgumentException e) 
            {
                Console.WriteLine(e.Message);
            }

            try
            {
                Console.Write($"Result from Get(VAL1): ");
                cacheObject.Get("VAL1");
                cacheObject.PrintDictionary();
                Thread.Sleep(500);
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine();


            Thread.Sleep(1500);
            Console.WriteLine("After 1.5 seconds:");
            cacheObject.PrintDictionary();
        }
    }
}