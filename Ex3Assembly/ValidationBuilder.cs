using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ex3Assembly
{

    public sealed class ValidationNoDataException:Exception
    {
        public ValidationNoDataException(string message): base($"ValidationNoDataException: {message}") {  }
    }

    public class Validator<T>
    {
        Validation<T>? _item;

        public Validator(Validation<T> firstItem)
        {
            _item=firstItem;
        }
    }
    public class ValidationBuilder<T>
    {
        Validator<T>? _firstItem = null;
        Validation<T>? _lastItem = null;

       
        //сделать доп класс - validator. Как в аллокаторах - ссылка на первый элемент
        //билдер возвращает объект который потом не может модифицировать
        public ValidationBuilder<T> AddValidator(Func<T, bool> task)
        {
            Validation<T> validation = new(task);
            if (_firstItem == null)
            {
                Validator<T> validator = new(validation);
                _lastItem = validation;
            }
            else
            {
                _lastItem!.Successor = validation;
                _lastItem = validation;
            }

            return this;
        }

        public Validator<T> Build()
        {
            if (_firstItem == null)
                throw new ValidationNoDataException("no objects were added");
            return _firstItem!;
        }
    }
}
