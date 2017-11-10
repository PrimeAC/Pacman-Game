using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net.Sockets;

using RemoteServices;
using System.Net;


namespace pacman {
    class Client {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            int port = FreeTcpPort();
            TcpChannel chan = new TcpChannel(port);
            ChannelServices.RegisterChannel(chan, false);

            // Alternative 1 for service activation
            ClientServices service = new ClientServices();
            RemotingServices.Marshal(service, "Client",
                typeof(ClientServices));

            //Alternative 2
            //RemotingConfiguration.RegisterWellKnownServiceType(
            //    typeof(ClientServices), "Client",
            //    WellKnownObjectMode.Singleton);

            IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://localhost:8086/Server");
            server.RegisterClient(port.ToString());

            Console.WriteLine(service.start);
            while (service.start != true)
            {
                continue;
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(service.getGameRate(), server, port));

        }

        private static int FreeTcpPort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }

        public class ClientServices : MarshalByRefObject, IClient
        {
            internal Boolean start = false;
            internal string gameRate;
            public string port;

            internal ClientServices()
            {
            }

            public void setPort(string port)
            {
                this.port = port;
            }

            public string getPort()
            {
                return this.port;
            }

            public void startGame(string gameRate)
            {
                this.gameRate = gameRate;
                this.start = true;
            }

            public string getGameRate()
            {
                return this.gameRate;
            }
        }

    }
}
