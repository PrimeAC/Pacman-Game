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
        List<IClient> getClients();
        //void sendMove(string clientPort, string move);
    }

    public interface IClient
    {
        void startGame();
        void setPort(string port);
        string getPort();
        //void updateGameState();
    }
}
