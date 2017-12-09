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
    public static class Client {

        static object _lockclient = new Object();

        public static string executionPath()
        {
            return @Environment.CurrentDirectory + "/pacman.exe";
        }

        static Form1 form;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {

            string url = args[0];
            string[] urlSplit = url.Split(':', '/');

            int port = Int32.Parse(urlSplit[4]);
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

        delegate void DelAddMsg(string mensagem, int[] vector);
        delegate void DelUpdateGame(Dictionary<string, int[]> pacmans, Dictionary<int, int[]> ghosts, Dictionary<int, int[]> coins);
        delegate void DelInitGame(Dictionary<string, int[]> pacmans);
        delegate void DelAddVector(int[] vector);

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
                //DelUpdateGame DelUpdateGame = new DelUpdateGame(form.updateGame);
                //DelUpdateGame(pacmans, ghosts, coins);
                //form.updateGame(pacmans, ghosts, coins);
                form.Invoke(new DelUpdateGame(form.updateGame), pacmans, ghosts, coins);
            }

            public void initGame(Dictionary<string, int[]> pacmans)
            {
                form.Invoke(new DelInitGame(form.initializeGame), pacmans);
            }

            public void fail(string s)
            {
                Console.WriteLine(s);
            }

            public void MsgToClient(string mensagem, int[] vector)
            {
                // thread-safe access to form
                form.Invoke(new DelAddMsg(form.AddMsg), mensagem, vector);
            }

            public void SendMsg(string mensagem, int[] vector)
            {
                messages.Add(mensagem);
                //ThreadStart ts = new ThreadStart(this.BroadcastMessage);
                //Thread t = new Thread(ts);
                Thread t = new Thread(() => BroadcastMessage(vector));
                t.Start();
            }
            public void BroadcastMessage(int[] vector)
            {
                string MsgToBcast;
                clients = form.getClients();
                lock (this)
                {
                    MsgToBcast = messages[messages.Count - 1];
                }
                for (int i = 0; i < clients.Count; i++)
                {
                    try
                    {
                        ((IClient)clients[i]).MsgToClient(MsgToBcast, vector);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed sending message to client. Removing client. " + e.Message);
                        clients.RemoveAt(i);
                    }
                }
            }

            public void requestMessage(int[] vector)
            {
                Thread t = new Thread(() => BroadcastVector(vector));
                t.Start();
            }

            public void BroadcastVector(int[] vector)
            {
                clients = form.getClients();
                for (int i = 0; i < clients.Count; i++)
                {
                    try
                    {
                        ((IClient)clients[i]).vectorToClient(vector);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed sending message to client. Removing client. " + e.Message);
                        clients.RemoveAt(i);
                    }
                }
            }

            public void vectorToClient(int[] vector)
            {
                // thread-safe access to form
                form.Invoke(new DelAddVector(form.gotVector), vector);
            }

            public List<string> getMessages()
            {
                return messages;
            }

        }

    }
}
