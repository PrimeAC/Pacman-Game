using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using RemoteServices;
using System.Timers;

namespace Server
{
    static class Server
    {
        private static string MSEC_PER_ROUND;
        private static int NUM_PLAYERS;
        public static Dictionary<string, string> moves = new Dictionary<string, string>();
        public static List<IClient> clients;

        private static System.Timers.Timer aTimer;

        static void Main(string[] args)
        {

            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, false);

            //Alternative 1
            //RemotingConfiguration.RegisterWellKnownServiceType(
            //    typeof(ServerServices), "Server",
            //    WellKnownObjectMode.Singleton);

            //Alternative 2 
            ServerServices service = new ServerServices();
            RemotingServices.Marshal(service, "Server",
                typeof(ServerServices));

            System.Console.WriteLine("Please enter the desired game rate:");
            MSEC_PER_ROUND = System.Console.ReadLine();

            System.Console.WriteLine("Please enter the number of players:");
            NUM_PLAYERS = Int32.Parse(System.Console.ReadLine());

            while (clients.Count != NUM_PLAYERS)
            {
                continue;
            }

            foreach (IClient client in clients.ToList())
            {
                System.Console.WriteLine(client.GetHashCode());
                client.startGame(MSEC_PER_ROUND, NUM_PLAYERS.ToString());
            }

            System.Console.WriteLine("Press <enter> to terminate game server...");
            System.Console.ReadLine();
        }

        class ServerServices : MarshalByRefObject, IServer
        {
            internal ServerServices()
            {
                clients = new List<IClient>();
            }

            public void RegisterClient(string NewClientPort)
            {
                Console.WriteLine("New client listening at " + "tcp://localhost:" + NewClientPort + "/Client");
                IClient newClient =
                    (IClient)Activator.GetObject(
                           typeof(IClient), "tcp://localhost:" + NewClientPort + "/Client");
                newClient.setPort(NewClientPort);
                clients.Add(newClient);
            }

            public List<IClient> getClients()
            {
                return clients;
            }

            public void sendMove(string port, string move)
            {
                moves.Add(port, move);

                Console.WriteLine("timer");
                foreach (KeyValuePair<string, string> entry in moves.ToList())
                {
                    foreach (IClient client in clients)
                    {
                        client.updateGameState(entry.Value);
                    }
                }
                moves.Clear();
            }
        }
    }
}
