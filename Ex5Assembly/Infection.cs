using LogProject;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using static Ex5Assembly.Infection;

namespace Ex5Assembly
{
    //допилить моменты. сделать большой файл
    public class Infection : IDisposable
    {
        private Logger _logger;
        private Mutex _mutex = new Mutex();
        //dispose + в очереди тоже (првоерить есть ли disposable)
        private float _probabilityOfGettingSick;
        private float _probabilityOfRecovery;
        private Random _random = new();
        List<Person> Society { get; set; } = new List<Person>();
        public int TimerTick { get; set; }

        ~Infection()
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
            _mutex.Dispose();
            _logger.Dispose();
            Disposed = true;
        }







        public float ProbabilityOfGettingSick
        {
            get => _probabilityOfGettingSick;
            set
            {
                if (value > 0 && value <= 1)
                    _probabilityOfGettingSick = value;
                else
                    throw new ArgumentException("Incorrect propability of getting sick");
            }
        }
        public float ProbabilityOfRecovery
        {
            get => _probabilityOfRecovery;
            set
            {
                if (value >= 0 && value <= 1)
                    _probabilityOfRecovery = value;
                else
                    throw new ArgumentException("Incorrect propability of recovery");
            }
        }

        public enum PersonState
        {
            Healthy, 
            Sick,
            Recovered
        }

        public class Person
        { 
            public int NameId { get; init; }
            public PersonState State { get; set; }
            public HashSet<Person> Surrounding { get; set; } = new HashSet<Person>(); 
            //Всегда когда есть Hash - переопределение GetHashCode. Equals
            //По умолчанию всегда Equals
            public Person(int nameId, PersonState state=PersonState.Healthy)
            {
                NameId = nameId;
                State=state;
            }
            public override bool Equals(object? obj)
            {
                if (obj == null)
                    return false;
                if ((obj as Person)!.State != State)
                    return true;
                if ((obj as Person)!.NameId != NameId)
                    return true;
                if ((obj as Person)!.Surrounding != Surrounding)
                    return true;
                return false;
            }

            public override int GetHashCode()
            {
                int result = State.GetHashCode() + NameId.GetHashCode() + Surrounding.GetHashCode();
                return result;
            }
            public override string ToString()
            {
                switch(State)
                {
                    case PersonState.Healthy:
                        return $"H";
                    case PersonState.Sick:
                        return $"S";
                    case PersonState.Recovered:
                        return $"R";
                    default: return nameof(Person);
                }
            }
        }


        public Infection(string path, float probabilityOfRecovery, float probabilityOfGettingSick, Logger logger, int timerTick=1000)
        {
            ProbabilityOfGettingSick = probabilityOfGettingSick;
            ProbabilityOfRecovery = probabilityOfRecovery;
            parsingDataFromJson(path);
            _logger = logger;
            TimerTick = timerTick;
        }

        async public Task SimulateAnInfection(int days) //случайная вспышка заболевания
        {
            try
            {


                _mutex.WaitOne();

                Person? lastPerson = null;
                bool infected = false;

                foreach (var person in Society.Where(p => p.State == PersonState.Healthy))
                {
                    lastPerson = person;
                    if (_random.Next(0, 100) <= ProbabilityOfGettingSick * 100)
                    {
                        person.State = PersonState.Sick;
                        if (infected) break;
                        infected = true;

                    }
                }

                if (!infected && lastPerson != null) lastPerson.State = PersonState.Sick;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }

            await InfectionTick(days);
        }

        async Task InfectionTick(int days)
        {
            for (int i=0; i<days || days==-1; ++i)
            {

                logState(i);
                float probabilityOfMeeting = 0;
                try
                {
                    _mutex.WaitOne();

                    foreach (var person in Society.Where(p => p.State == PersonState.Sick))
                    {
                        //Заражаем здоровых       
                        foreach (var surrPerson in person.Surrounding.Where(p => p.State == PersonState.Healthy))
                        {
                            probabilityOfMeeting = (float)(_random.Next(50, 100) / 100.0);
                            var m = _random.Next(0, 100);

                            if (m <= probabilityOfMeeting * ProbabilityOfGettingSick * 100)
                            {
                                surrPerson.State = PersonState.Sick;
                            }
                        }

                        //Выздоравливаем
                        var n = _random.Next(0, 100);

                        if (n <= ProbabilityOfRecovery * 100)
                        {
                            person.State = PersonState.Recovered;
                        }
                    }
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
               


                if (Society.Any(p => p.State == PersonState.Recovered) && Society.Any(p => p.State == PersonState.Sick) == false)
                {
                    logState(i);
                    break;
                }

                await Task.Delay(TimerTick);
            }
        }


        public IEnumerable<Person> getUninfectedPersons()
        {
            return Society.Where(p => p.State == PersonState.Healthy);
        }

        public IEnumerable<Person> getRecoveredPersons()
        {
            return Society.Where(p => p.State == PersonState.Recovered);
        }


        public IEnumerable<Person> getRecoveredPersons_SurroundingNotRecovered()
        {
            return Society.Where(
                p => 
                p.State == PersonState.Recovered &&
                p.Surrounding.All(q => q.State!=PersonState.Recovered)
                );
        }

        public IEnumerable<Person> getUninfectedPersons_SurroundingInfected()
        {
            return Society.Where(
                p =>
                p.State == PersonState.Healthy &&
                p.Surrounding.All(q => q.State != PersonState.Healthy)
                );
        }


        void logState(int day)
        {

           StringBuilder stringBuilder = new StringBuilder($"Day {day}:\n");
           foreach (var person in  Society)
           {
                stringBuilder.Append($"{person}:\t ");
                foreach (var sperson in person.Surrounding)
                {
                    stringBuilder.Append($"{sperson}, ");
                }

                stringBuilder.Append("\n");
           }

            _logger.Log(stringBuilder.ToString());
         
        }

         void parsingDataFromJson(string path)
        {
           parsingDataFromJson_Society(path);
           parsingDataFromJson_Surrounding(path);
        }

        void parsingDataFromJson_Society(string path)
        {
            _mutex.WaitOne();
            try
            {
                using StreamReader fs = new(path);

                string? temp;
                int i = -1;
                while ((temp = fs.ReadLine()) != null)
                {
                    if (temp.Contains("Person:"))
                    {
                        Society.Add(new Person(++i));
                    }
                    
                }

               
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        void parsingDataFromJson_Surrounding(string path)
        {
            try
            {
                _mutex.WaitOne();
                using StreamReader fs = new(path);

                string? temp;
                int i = 0;
                while ((temp = fs.ReadLine()) != null)
                {
                    if (temp.Contains("Person:"))
                    {
                        foreach (var indexStr in temp.Replace("Person:", "").Split(','))
                        {
                            int index = int.Parse(indexStr);
                            if (index != i)
                            {
                                Society[i].Surrounding.Add(Society[index]);
                            }
                        }
                        ++i;
                    }
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    }
}
