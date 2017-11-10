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
            System.Console.WriteLine("Please enter the desired game rate:");
            MSEC_PER_ROUND = System.Console.ReadLine();

            System.Console.WriteLine("Please enter the number of players:");
            NUM_PLAYERS= Int32.Parse(System.Console.ReadLine());

            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerServices), "Server",
                WellKnownObjectMode.Singleton);

            while(ServerServices.clients.Count != NUM_PLAYERS)
            {
                continue;
            }


            System.Console.WriteLine("Press <enter> to terminate game server...");
            System.Console.ReadLine();
        }

        class ServerServices : MarshalByRefObject, IServer
        {

            internal static List<IClient> clients;

            ServerServices()
            {
                clients = new List<IClient>();
            }

            public string RegisterClient(string NewClientPort)
            {
                Console.WriteLine("New client listening at " + "tcp://localhost:" + NewClientPort + "/Client");
                IClient newClient =
                    (IClient)Activator.GetObject(
                           typeof(IClient), "tcp://localhost:" + NewClientPort + "/Client");
                clients.Add(newClient);
                return MSEC_PER_ROUND;
            }
        }
    }
}
