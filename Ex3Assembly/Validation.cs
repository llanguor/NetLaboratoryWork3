using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Ex3Assembly
{
    //sealed !!!!!!!
    public sealed class FailedValidateException : Exception
    {
        public FailedValidateException(string message) : base("FailedValidateException: "+message) { }
    }
    
    public sealed class Validation<T>(Func<T, bool> task)
    {
        public Validation<T>? Successor { get; set; }
        public int Number { get; set; }
        Func<T, bool> _task = task;

        public void Validate(T input)
        {
            var exceptionsList = new List<Exception>();
            if (!_task(input)) exceptionsList.Add(
                new FailedValidateException($"Failed validation"));

            try
            {
                Successor?.Validate(input);
            }
            catch (AggregateException e)
            {
                exceptionsList.AddRange(e.InnerExceptions.AsEnumerable());
            }

            throw new AggregateException(exceptionsList);
        } 
    }
}
