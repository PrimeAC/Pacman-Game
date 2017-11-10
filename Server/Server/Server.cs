using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using RemoteServices;

namespace Server
{
    static class Server
    {
        private static string MSEC_PER_ROUND;
        private static int NUM_PLAYERS;

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


            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, false);
            ServerServices service = new ServerServices();

            RemotingServices.Marshal(
                service,"Server",typeof(ServerServices));

            while (service.clients.Count != NUM_PLAYERS)
            {
                continue;
            }

            foreach (IClient client in service.clients.ToList())
            {
                System.Console.WriteLine(client.GetHashCode());
                client.startGame(MSEC_PER_ROUND);
            }
            System.Console.WriteLine("Press <enter> to terminate game server...");
            System.Console.ReadLine();
        }

        class ServerServices : MarshalByRefObject, IServer
        {

            internal List<IClient> clients;

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
        }
    }
}
