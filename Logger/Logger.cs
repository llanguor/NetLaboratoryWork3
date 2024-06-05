using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System.Text;
using System.Reflection.PortableExecutable;

namespace LogProject
{
    public class Logger: IDisposable
    {
        FileStream _stream;
        bool _consoleOutput;
        Mutex _mutex = new();
        public Logger(string path, bool consoleOutput=true) 
        {
            _stream = new FileStream(path, FileMode.OpenOrCreate);
            _consoleOutput = consoleOutput;
        }

        async public void Log(string msg, bool forceConsoleOutput=false)
        {
          //  mutex.WaitOne();
            byte[] buffer = Encoding.Default.GetBytes(msg);
            await _stream.WriteAsync(buffer, 0, buffer.Length);
            if(_consoleOutput || forceConsoleOutput) Console.WriteLine(msg);
          //  mutex.ReleaseMutex();
        }


        ~Logger()
        {
            Dispose(true);
        }

        bool Disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  
        }
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;
            //if (disposing){
            // Освобождаем управляемые ресурсы

            //} //освобождаем неуправляемые ресурсы

            _stream.Close();
            _stream.Dispose();
            _mutex.Dispose();
            Disposed = true;
        }
    }
}
