using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteServices
{
   
    public interface IServer
    {
        List<IClient> getClients();
        void RegisterClient(string NewClientPort);
        //void sendMove(string clientPort, string move);
    }

    public interface IClient
    {
        void setPort(string port);
        string getPort();
        void startGame(string gameRate);
        //void updateGameState();
    }
}
