using Ex5Assembly;
using LogProject;
using System.Text;
using System.Threading;

namespace Ex5GenerateRandomPersons
{
    public class InfectionGenerateRandom : IDisposable
    {
        string? _path;
        Logger? _logger;

        public void GenerateFile(string path, int count, float probability)
        {
            if (File.Exists(_path)) File.Delete(_path);
            _path = path;
            _logger = new Logger(path, false);

            StringBuilder sb = new();
            Random r = new();
            int randomvalue;
            using Mutex mutex = new();

            for(int i = 0; i < count; ++i)
            {
                sb.Append("Person:");
                bool isRecord = false;
                for (int j = 0; j < count; ++j)
                {
                    if (i == j) continue;
                    randomvalue = r.Next(0, 100);
                    if(randomvalue< probability*100)
                    {
                        sb.Append(j);
                        sb.Append(',');
                        isRecord = true;
                    }
                }
                if (isRecord)
                {
                    sb.Replace(",", null, sb.Length - 1, 1);
                    if (i != count - 1) sb.Append("\n");

                    mutex.WaitOne();
                    _logger.Log(sb.ToString());
                    mutex.ReleaseMutex();
                }

                sb.Clear();
            }
            _logger.Dispose();
        }

        public async Task SimulateAnInfection(float probabilityOfRecovery, float probabilityOfGettingSick, int days, Logger logger, int timerTick=1000)
        {
            if (_path == null) throw new ArgumentException("Random file is not generated now");
            using Infection infection = new Infection(_path, probabilityOfRecovery, probabilityOfGettingSick, logger, timerTick);
            await infection.SimulateAnInfection(days);
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
