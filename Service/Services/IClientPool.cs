using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Core;

namespace Service.Services
{
    public interface IClientPool
    {
        T Get<T>(IMessage msg);
    }


}
