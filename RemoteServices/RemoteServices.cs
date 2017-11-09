using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteServices
{
   
    public interface IServer
    {
        string RegisterClient(string NewClientPort);
    }

    public interface IClient
    {
        string GetMove();
    }
}
