using Ex3Assembly;
using System.Runtime.CompilerServices;

namespace Ex3Launch
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var validation1 = new ConcreteValidationBuilder1().Build();
            var validation2 = new ConcreteValidationBuilder2().Build();
           

            Random r = new Random();
            for(int i = 0; i < 5; i++)
            {
                var value = r.Next(0, 5);
                try
                {
                    validation1.Validate(value);
                }
                catch (AggregateException ex)
                {
                    printAggregateException($"Result for value {i}", ex);
                }
            }
            try
            {
                validation2.Validate("Lel");
            }
            catch(AggregateException ex)
            {
                printAggregateException($"Result for value \"Lel\":", ex);
            }
        }

        static void printAggregateException(string label, AggregateException exceptions)
        { 
            Console.WriteLine(label);
            foreach (var e in exceptions.InnerExceptions)
            { 
                Console.WriteLine(e.Message);
            }
            Console.WriteLine();
        }
    }


    internal class ConcreteValidationBuilder1
    {
        public Validation<int> Build()
        {
            return new ValidationBuilder<int>()
                .AddValidator(CheckDivisibleBy2)
                .AddValidator(CheckDivisibleBy3)
                .AddValidator(CheckPoitive)
                .AddValidator(CheckNegative)
                .Build();
        }

        bool CheckDivisibleBy2(int number)
        {
            return number % 2==0;
        }

        bool CheckDivisibleBy3(int number)
        {
            return number%3==0;
        }
            
        bool CheckPoitive(int number)
        {
            return number > 0;
        }

        bool CheckNegative(int number)
        {
            return number < 0;
        }
    }

    internal class ConcreteValidationBuilder2
    {
        public Validation<string> Build()
        {
            return new ValidationBuilder<string>()
                .AddValidator(CheckZero)
                .AddValidator(CheckPoitive)
                .AddValidator(CheckNegative)
                .Build();
        }

        bool CheckZero(string s)
        {
            return s.Length == 0;
        }

        bool CheckPoitive(string s)
        {
            return s.Length > 3;
        }

        bool CheckNegative(string s)
        {
            return s.Length < 3;
        }
    }
}