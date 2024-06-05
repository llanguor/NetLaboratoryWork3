using LogProject;
using System.Threading;

namespace Ex5GenerateRandomPersons
{
    public class InfectionGenerateRandom : IDisposable
    {
        string _path;
        Logger _logger;
        public InfectionGenerateRandom(string path)
        {
            _logger = new Logger(path);
            _path = path;
        }

        public void GenerateFile()
        {

        }

        public void ExecuteInfection()
        {

        }



        ~InfectionGenerateRandom()
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
            _logger.Dispose();
            Disposed = true;
        }
    }
}
