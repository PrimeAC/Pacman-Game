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
        void RegisterClient(string NewClientIP, string NewClientPort);
        void sendMove(string ip, string port, string move);
        void readyClient();
    }

    public interface IClient
    {
        void setPort(string port);
        string getPort();
        void setIP(string ip);
        string getIP();
        void startGame(string gameRate, string numPlayers);
        string getGameRate();
        string getNumPlayers();
        void updateGameState(Dictionary<string, int[]> pacmans, Dictionary<int, int[]> ghosts, Dictionary<int, int[]> coins);
        void initGame(Dictionary<string, int[]> pacmans);
        void MsgToClient(string message);
        void SendMsg(string message);
        void BroadcastMessage();
        List<string> getMessages();
    }
}
