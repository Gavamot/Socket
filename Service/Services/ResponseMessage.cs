using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.DataClasses;

namespace Service.Services
{
    public class ResponseMessage<T>
    {

        public ResponseMessage() { }
        public ResponseMessage(DateTime actual, T data)
        {
            Actual = actual;
            Data = data;
        }

        public DateTime Actual { get; set; }
        public T Data { get; set; }
    }
}
