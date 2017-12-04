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
using System.Threading;

namespace Server
{
    static class Server
    {
        private static string MSEC_PER_ROUND;
        private static int NUM_PLAYERS;
        public static List<IClient> clients;
        public static GameEngine engine;
        private static bool gamestarted = false;
        private static bool gamesettings = false;  //game settings is true when the game rate and the num of players is known


        static object _lock = new Object();
        static object _lock1 = new Object();
        

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
            if (MSEC_PER_ROUND == "")
            {
                MSEC_PER_ROUND = "20";
            }

            System.Console.WriteLine("Please enter the number of players:");
            NUM_PLAYERS = Int32.Parse(System.Console.ReadLine());

            gamesettings = true;

            Console.WriteLine("Clients number -> " + clients.Count);

            while (clients.Count != NUM_PLAYERS)
            {
                lock (_lock)
                {
                    Monitor.Wait(_lock);
                }
            }
            

            foreach (IClient client in clients.ToList())
            {
                System.Console.WriteLine(client.GetHashCode());
                client.startGame(MSEC_PER_ROUND, NUM_PLAYERS.ToString());
            }
            
            while (service.ready < NUM_PLAYERS)
            {
                lock (_lock1)
                {
                    Monitor.Wait(_lock1);
                }
            }
            gamestarted = true;   //indicates that a game as started
            engine.start();
            System.Threading.Thread.Sleep(1000);
            engine.startTimer(MSEC_PER_ROUND);
            engine.seePacmans();
            

            System.Console.WriteLine("Press <enter> to terminate game server...");
            System.Console.ReadLine();
        }

        delegate void DelSendMoves(string ip, string port, string move);

        class ServerServices : MarshalByRefObject, IServer
        {
            internal int ready = 0;
            internal ServerServices()
            {
                clients = new List<IClient>();
                engine = new GameEngine(clients);

            }

            public void RegisterClient(string NewClientIP, string NewClientPort)
            {
                if(gamestarted == false && gamesettings == true)  
                {
                    Console.WriteLine("New client listening at " + "tcp://" + NewClientIP + ":" + NewClientPort + "/Client");
                    IClient newClient =
                        (IClient)Activator.GetObject(
                               typeof(IClient), "tcp://" + NewClientIP + ":" + NewClientPort + "/Client");
                    newClient.setPort(NewClientPort);
                    newClient.setIP(NewClientIP);
                    clients.Add(newClient);
                    engine.setPacmans(NewClientIP, NewClientPort);
                    engine.seePacmans();

                    lock (_lock)
                    {
                        Monitor.Pulse(_lock);
                    }
                }
                else
                {
                    IClient newClient =
                        (IClient)Activator.GetObject(
                               typeof(IClient), "tcp://" + NewClientIP + ":" + NewClientPort + "/Client");
                    newClient.fail("Game already started.");
                }
                
            }

            public List<IClient> getClients()
            {
                return clients;
            }

            public void sendMove(string ip, string port, string move)
            {
                //engine.setMoves(ip, port, move);
                DelSendMoves delSendMoves = new DelSendMoves(engine.setMoves);
                delSendMoves(ip, port, move);
            }

            public void readyClient()
            {
                ready++;
                if(ready == NUM_PLAYERS)
                {
                    lock (_lock1)
                    {
                        Monitor.Pulse(_lock1);
                    }
                }
            }
        }
    }
}
