using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Reflection.PortableExecutable;

namespace TokenizationHelperTools
{
    public class TokenizationHelper : IDisposable, IEnumerable<string>
    {
        private StreamReader _reader;
        private HashSet<char> _separators;
        private bool _disposed = false;

        private StreamReader Reader => _reader;
        private HashSet<char> Separators => _separators;
        private bool Disposed
        {
            get => _disposed;
            set => _disposed = value;
        }

        public TokenizationHelper(string path, HashSet<char> separators)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Incorrect parameter '{nameof(path)}': file not found at path {path}");
            if (separators.Count == 0)
                throw new ArgumentNullException($"Incorrect parameter: blank list of separators");

            _separators = separators;
            _reader = new StreamReader(path);
        }

        public void Tokenize(Action<string> action)
        {
            foreach (var item in this)
            {
                action(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            var builder = new StringBuilder();
            char ch;
            while (Reader.Peek() >= 0)
            {
                ch = (char)Reader.Read();

                if (Separators.Contains(ch) || ch == '\n')
                {
                    if (builder.Length > 0)
                    {
                        yield return builder.ToString();
                        builder.Clear();
                    }
                }
                else
                {
                    builder.Append(ch);
                }

                if (ch == '\n') yield return "\n";
            }

            if (Reader.Peek() < 0 && builder.Length > 0) yield return builder.ToString();
        }

        ~TokenizationHelper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  //Не позволит вызвать dispose в finalize после try
        }

        //шаблон из документации microsoft
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;
            // if (disposing)
            // {
            // Освобождаем управляемые ресурсы
            // }

            // освобождаем неуправляемые объекты

            Reader.Close();
            Reader.Dispose();
            Disposed = true;
        }
    }
}