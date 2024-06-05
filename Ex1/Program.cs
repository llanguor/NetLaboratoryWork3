using Ex1Assembly;

namespace Ex1Launch
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HashSet<char> separators = new HashSet<char>() { ';' };
            Tokenization t = new Tokenization("..\\..\\..\\Resources\\Input.txt", separators);
            t.Tokenize(printTokens);
        }

        static void printTokens(string text)
        {
            Console.WriteLine(text);
        }
    }
}
