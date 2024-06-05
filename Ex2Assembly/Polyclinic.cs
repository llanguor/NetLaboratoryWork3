using LogProject;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace Ex2Assembly
{
    public class Polyclinic:IDisposable
    {   //модификаторы доступа явно
        private readonly Random r = new();
        private readonly Logger _logger;

        private int _maxReceiptTime;
        private int _doctorsCount;
        private int _viewingCapacity;
        private int _standingNappingTime;
        private int _infectedNappingTime;
        private int _generateMaxNappingTime;

        //всегда если mutex waitone делать finally
        private VisitorType _viewingState;
        private ConcurrentQueue<Visitor> _viewingQueue = new();
        private List<Visitor> _outdoorsList = new();
        private List<Task> _doctorsList = new();
        private Mutex _outdoorsMutex = new();
        private Mutex _doctorsMutex = new();
        private Mutex _logMutex = new();

        //Task _visitorGeneratorTask;
        //Task _spreaderOfDiseaseTask;
        //Task _advancementerOfTheQueueTask;
        //Task _distributorToDoctorsTask;

        //если у объекта есть метод dispose то его нужно чистить в реализации Dispose класса
        //если взято у ОС надо Dispose 
        //Task Mutex
        //Если выполняется постоянно то лучше Thread
        //Второй раз Task запустить не выйдет
        //Не хранить их как поле

        ~Polyclinic()
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
            _doctorsMutex.Dispose();
            _logMutex.Dispose();
            _outdoorsMutex.Dispose();
           
            Disposed = true;
        }
        void LogMessage(string message, bool fromExecutedMethod = false)
        {
            try
            {
                _logMutex.WaitOne();

                _logger.Log(message);
                LogOutdoorsList();
                LogViewingQueue();
                LogCountOfBusyDoctors(fromExecutedMethod);
                _logger.Log("");

            }finally { _logMutex.ReleaseMutex(); }

        }

        void LogCountOfBusyDoctors(bool fromExecutedMethod = false)
        {
            int count = 0;
            foreach (var d in _doctorsList)
            {
                if (!d.IsCompleted) ++count;
            }
            if (fromExecutedMethod) --count;
            _logger.Log($"Count of busy doctors: {count}");
        }
        void LogOutdoorsList()
        {
            StringBuilder sb = new("Outdoors list: ");
            foreach (var x in _outdoorsList)
            {
                sb.Append(x.Type == VisitorType.Sick ? "sick " : "advice ");
            }
            _logger.Log(sb.ToString());
        }
        void LogViewingQueue()
        {
            StringBuilder sb = new("Viewing queue: ");
            foreach (var x in _viewingQueue)
            {
                sb.Append(x.Type == VisitorType.Sick ? "sick " : "advice ");
            }
            _logger.Log(sb.ToString());
        }

        enum VisitorType
        {
            Sick = 1,
            Advice = 2
        }
        class Visitor(VisitorType visitorType, Func<Task> func)
        {
            public VisitorType Type = visitorType;
            public Func<Task> Task = func;
        }
        public int MaxReceiptTime
        {
            get => _maxReceiptTime;
            init
            {
                if (value < 1) throw new ArgumentException("Incorrect agrument of max receipt time");
                _maxReceiptTime = value;
            }
        }
        public int DoctorsCount
        {
            get => _doctorsCount;
            init
            {
                if (value < 1) throw new ArgumentException("Incorrect agrument of doctors count");
                _doctorsCount = value;
            }
        }
        public int ViewingCapacity
        {
            get => _viewingCapacity;
            init
            {
                if (value < 1) throw new ArgumentException("Incorrect agrument of viewing capacity");
                _viewingCapacity = value;
            }
        }
        public int StandingNappingTime
        {
            get => _standingNappingTime;
            init
            {
                if (value < 1) throw new ArgumentException("Incorrect agrument of standing napping time");
                _standingNappingTime = value;
            }
        }
        public int GenerateMaxNappingTime
        {
            get => _generateMaxNappingTime;
            init
            {
                if (value < 1) throw new ArgumentException("Incorrect agrument of generate max napping time");
                _generateMaxNappingTime = value;
            }
        }

        public int InfectedNappingTime
        {
            get => _infectedNappingTime;
            init
            {
                if (value < 1) throw new ArgumentException("Incorrect agrument of infected napping time");
                _infectedNappingTime = value;
            }
        }

        public Polyclinic(int maxReceiptTime, int doctorsCount, int viewingCapacity, int standingNappingTime, int generateMaxNappingTime, int infectedNappingTime, Logger logger)
        {
            MaxReceiptTime = maxReceiptTime;
            DoctorsCount = doctorsCount;
            ViewingCapacity = viewingCapacity;
            StandingNappingTime = standingNappingTime;
            GenerateMaxNappingTime = generateMaxNappingTime;
            _infectedNappingTime = infectedNappingTime;

            _logger= logger;
            _logger.Log("Polyclinic processes started\n");
                 
        }

        public void StartExecution()
        {
            var visitorGeneratorThread = new Thread(VisitorGenerator)
            {
                IsBackground = true
            };
            visitorGeneratorThread.Start();
            var spreaderOfDiseaseThread = new Thread(SpreaderOfDisease)
            {
                IsBackground = true
            };
            spreaderOfDiseaseThread.Start();
            var advancementOfTheQueueThread = new Thread(AdvancementerOfTheQueue)
            {
                IsBackground = true
            };
            advancementOfTheQueueThread.Start();
            var distributorToDoctorsThread = new Thread(DistributorToDoctors)
            {
                IsBackground = true
            };
            distributorToDoctorsThread.Start();
            visitorGeneratorThread.Join();
        }
  

        int GetTimeOfReceipt()
        {
            return r.Next(10, MaxReceiptTime);
        }
        async Task visitorReceipt()
        {
            int time = GetTimeOfReceipt();
            await Task.Delay(time);
            LogMessage($"DOCTOR FINISHED: {time}m", true);
        }

        async void VisitorGenerator()
        {
            int count, countSick, countAdvice;
            int generatorNappingTime;
            while (true)
            {
                count = r.Next(1, 4);
                countSick = 0;
                countAdvice = 0;
                try
                {
                    _outdoorsMutex.WaitOne();
                    for (int i = 0; i < count; ++i)
                    {
                        Visitor v = new(r.Next(0, 2) == 0 ? VisitorType.Sick : VisitorType.Advice, visitorReceipt);

                        switch (v.Type)
                        {
                            case VisitorType.Sick:
                                ++countSick;
                                break;
                            case VisitorType.Advice:
                                ++countAdvice;
                                break;
                        }

                        _outdoorsList.Add(v);
                    }
                }
                finally
                {
                    _outdoorsMutex.ReleaseMutex();
                }
               
                LogMessage($"ARRIVED: {countSick} sick, {countAdvice} advice");
                generatorNappingTime = r.Next(10, _generateMaxNappingTime);
                await Task.Delay(generatorNappingTime);
            }
        }


        async void SpreaderOfDisease()
        {
            bool isInfected = false;
            while (true)
            {
                if (_outdoorsList.Count == 0)
                {
                    await Task.Delay(_standingNappingTime);
                    continue;
                }

                try
                {
                    _outdoorsMutex.WaitOne();
                    isInfected = false;
                    for (int i = 0; i < _outdoorsList.Count; ++i)
                    {
                        if (_outdoorsList[i].Type == VisitorType.Sick)
                        {
                            if (i > 0 && _outdoorsList[i - 1].Type != VisitorType.Sick && r.Next(0, 2) != 0)
                            {
                                _outdoorsList[i - 1].Type = VisitorType.Sick;
                                isInfected = true;
                            }

                            if (i < _outdoorsList.Count - 1 && _outdoorsList[i + 1].Type != VisitorType.Sick && r.Next(0, 2) != 0)
                            {
                                _outdoorsList[i + 1].Type = VisitorType.Sick;
                                isInfected = true;
                            }
                        }

                        if (isInfected) break;
                    }

                }
                finally
                {
                    _outdoorsMutex.ReleaseMutex();
                }
               
                if (isInfected)
                {
                    LogMessage("INFECTED");
                    await Task.Delay(_infectedNappingTime);
                }
                else
                {
                    await Task.Delay(_standingNappingTime);
                    continue;
                }
            }
        }

        async void AdvancementerOfTheQueue()
        {
            int searchedIndex;
            VisitorType? type=null;

            while (true)
            {
                if (_outdoorsList.Count == 0 || _viewingQueue.Count == _viewingCapacity)
                {
                    await Task.Delay(_standingNappingTime);
                    continue;
                }

                searchedIndex = -1;
                try
                {
                    _outdoorsMutex.WaitOne();
                    for (int i = 0; i < _outdoorsList.Count(); ++i)
                    {
                        if (_viewingQueue.Count == 0 || _outdoorsList[i].Type == _viewingState)
                        {
                            searchedIndex = i;
                            break;
                        }
                    }

                    if (searchedIndex != -1)
                    {
                        var findedVisitor = _outdoorsList[searchedIndex];
                        _outdoorsList.RemoveAt(searchedIndex);
                        

                        if (_viewingQueue.Count == 0)
                            _viewingState = findedVisitor.Type;

                        type = findedVisitor.Type;
                        _viewingQueue.Enqueue(findedVisitor);
                       
                    }
                   
                }
                finally
                {
                    _outdoorsMutex.ReleaseMutex();
                }

                if (searchedIndex!=-1)
                {
                    LogMessage($"TO VIEWINGROOM: {VisitorTypeToString((VisitorType)type!)}");
                }
                else
                {

                    await Task.Delay(_standingNappingTime);
                    continue;
                }
            }
        }


        async void DistributorToDoctors()
        {
            int completedTaskId;
            int randomVal;
            Visitor? visitor;

            while (true)
            {
                if (_viewingQueue.Count == 0)
                {
                    await Task.Delay(_standingNappingTime);
                    continue;
                }

                _viewingQueue.TryDequeue(out visitor);

                try
                {
                    _doctorsMutex.WaitOne();
                    if (_doctorsList.Count < _doctorsCount)
                    {
                        _doctorsList.Add(visitor!.Task());
                    }
                    else
                    {

                        completedTaskId = Task.WaitAny(_doctorsList.ToArray()); //сохранять сразу так. не конв. каждый раз
                        _doctorsList[completedTaskId] = visitor!.Task();

                        randomVal = r.Next(0, 10);
                        if (randomVal < 3)
                        {
                            //запрос помощи у второго доктора в особых случаях
                            completedTaskId = Task.WaitAny(_doctorsList.ToArray());
                            _doctorsList[completedTaskId] = visitor.Task();
                        }
                    }
                }
                finally
                {
                    _doctorsMutex.ReleaseMutex();
                }
                

                LogMessage($"TO DOCTOR: {VisitorTypeToString(visitor.Type)}");
            }
        }

        string VisitorTypeToString(VisitorType type)
        {
            return type == VisitorType.Sick ? "sick" : "advice";
        }
    }
}
