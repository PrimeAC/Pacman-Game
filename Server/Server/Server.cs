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
    class Server
    {

        static void Main(string[] args)
        {
            
            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerServices), "Server",
                WellKnownObjectMode.Singleton);
            System.Console.WriteLine("Press <enter> to terminate chat server...");
            System.Console.ReadLine();
        }

        class ServerServices : MarshalByRefObject, IServer
        {
            List<IClient> clients;

            ServerServices()
            {
                clients = new List<IClient>();
            }


            public string RegisterClient(string NewClientName)
            {
                Console.WriteLine("New client listening at " + "tcp://localhost:" + NewClientName + "/Client");
                IClient newClient =
                    (IClient)Activator.GetObject(
                           typeof(IClient), "tcp://localhost:" + NewClientName + "/Client");
                clients.Add(newClient);
                return "20";
            }
        }
    }
}
