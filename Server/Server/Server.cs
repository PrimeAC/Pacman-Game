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


        static object _lock = new Object();

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
            Console.WriteLine("vou parar");
            System.Threading.Thread.Sleep(1000);

            Console.WriteLine("ja voltei");
            engine.startTimer(MSEC_PER_ROUND);
            engine.seePacmans();

            System.Console.WriteLine("Press <enter> to terminate game server...");
            System.Console.ReadLine();
        }


        class ServerServices : MarshalByRefObject, IServer
        {
            internal ServerServices()
            {
                clients = new List<IClient>();
                engine = new GameEngine(clients);

            }

            public void RegisterClient(string NewClientIP, string NewClientPort)
            {
                Console.WriteLine("New client listening at " + "tcp://" + NewClientIP + ":" + NewClientPort + "/Client");
                IClient newClient =
                    (IClient)Activator.GetObject(
                           typeof(IClient), "tcp://" + NewClientIP + ":" + NewClientPort + "/Client");
                newClient.setPort(NewClientPort);
                newClient.setIP(NewClientIP);
                clients.Add(newClient);
                //engine.getPacmans().Add(NewClientIP + ":" + NewClientPort, new string[] { "8", engine.calculatePacmanPosY() });
                engine.setPacmans(NewClientIP, NewClientPort);
                //engine.getScore().Add(NewClientIP + ":" + NewClientPort, 0);
                //engine.setScore(NewClientIP, NewClientPort);

                
                foreach (IClient client in clients)
                {
                    Console.WriteLine("cliente no servidor "+ client.getIP()+":"+client.getPort());
                }
                foreach (IClient client in engine.getClients())
                {
                    Console.WriteLine("cliente no motor " + client.getIP() + ":" + client.getPort());
                }


                if (clients.Equals(engine.getClients()))
                {
                    Console.WriteLine("sao iguais");
                }

                lock (_lock)
                {
                    Monitor.Pulse(_lock);
                }
            }

            public List<IClient> getClients()
            {
                return clients;
            }

            public void sendMove(string ip, string port, string move)
            {
                //engine.getMoves().Add(ip+ ":" +port, move);
                engine.setMoves(ip, port, move);
                //engine.update();


                //foreach (KeyValuePair<string, string> kvp in moves)
                //{
                //    //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                //    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                //}

                //Console.WriteLine("timer");

                //foreach (KeyValuePair<string, string> entry in engine.getMoves().ToList())
                //{
                //    foreach (IClient client in clients)
                //    {
                //        client.updateGameState(engine.getPacmans(), engine.getGhosts(), engine.getCoins());
                //    }
                //}
                //engine.getMoves().Clear();
            }
        }
    }
}
