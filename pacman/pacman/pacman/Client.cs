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

        static object _lockclient = new Object();

        static Form1 form;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            int port = FreeTcpPort();
            string ip = GetLocalIPAddress();
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
            server.RegisterClient(ip, port.ToString());
            

            Console.WriteLine(service.start);
            while (service.start != true)
            {
                lock (_lockclient)
                {
                    Monitor.Wait(_lockclient);
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1(service.getGameRate(), service.getNumPlayers(), server, ip, port, service);
            //form = new Form1(server, ip, port, service);
            Application.Run(form);

        }

        private static int FreeTcpPort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        delegate void DelAddMsg(string mensagem);
        delegate void DelUpdateGame(Dictionary<string, int[]> pacmans, Dictionary<int, int[]> ghosts, Dictionary<int, int[]> coins);
        

        public class ClientServices : MarshalByRefObject, IClient
        {
            internal Boolean start = false;
            internal string gameRate;
            internal string numPlayers;
            public string port;
            public string ip;
            
            public static Form1 form;
            public List<IClient> clients;
            List<string> messages;
            
            internal ClientServices()
            {
                messages = new List<string>();
            }

            public void setPort(string port)
            {
                this.port = port;
            }

            public string getPort()
            {
                return this.port;
            }

            public void setIP(string ip)
            {
                this.ip = ip;
            }

            public string getIP()
            {
                return this.ip;
            }

            public void startGame(string gameRate, string numPlayers)
            {
                this.gameRate = gameRate;
                this.numPlayers = numPlayers;
                this.start = true;
                lock (_lockclient)
                {
                    Monitor.Pulse(_lockclient);
                }
                //form.initializeGame(gameRate, numPlayers);
            }

            public string getGameRate()
            {
                return this.gameRate;
            }

            public string getNumPlayers()
            {
                return this.numPlayers;
            }

            public void updateGameState(Dictionary<string, int[]> pacmans, Dictionary<int, int[]> ghosts, Dictionary<int, int[]> coins)
            {
                Console.WriteLine("entrei");
                //DelUpdateGame DelUpdateGame = new DelUpdateGame(form.updateGame);
                //DelUpdateGame(pacmans, ghosts, coins);
                //form.updateGame(pacmans, ghosts, coins);
                form.Invoke(new DelUpdateGame(form.updateGame), pacmans, ghosts, coins);
            }

            public void MsgToClient(string mensagem)
            {
                // thread-safe access to form
                form.Invoke(new DelAddMsg(form.AddMsg), mensagem);
            }

            public void SendMsg(string mensagem)
            {
                messages.Add(mensagem);
                ThreadStart ts = new ThreadStart(this.BroadcastMessage);
                Thread t = new Thread(ts);
                t.Start();
            }
            public void BroadcastMessage()
            {
                string MsgToBcast;
                clients = form.getServer().getClients();
                lock (this)
                {
                    MsgToBcast = messages[messages.Count - 1];
                }
                for (int i = 0; i < clients.Count; i++)
                {
                    try
                    {
                        ((IClient)clients[i]).MsgToClient(MsgToBcast);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed sending message to client. Removing client. " + e.Message);
                        clients.RemoveAt(i);
                    }
                }
            }

            public List<string> getMessages()
            {
                return messages;
            }

        }

    }
}
