using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting;
using RemoteServices;
using System.Diagnostics;


namespace pacman
{
    class PuppetMaster
    {

        static PuppetMasterWindow form;
        private static Dictionary<string, string> pidUrl = new Dictionary<string, string>();
        private static List<string> servers = new List<string>();
        private static List<string> clients = new List<string>();
        private static List<string> listPCS = new List<string>();

        [STAThread]
        static void Main()
        {
            //var th = new Thread(consoleApp);
            //th.Start();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new PuppetMasterWindow();
            Application.Run(form);
        }

        static string[] splitInputBox(string input)
        {
            string[] result = input.Split(' ');
            return result;
        }

        public static void read(String input)
        {

            string[] commands = splitInputBox(input);


            switch (commands[0])
            {
                case "StartClient":
                    startClient(commands[1], commands[2], commands[3], Int32.Parse(commands[4]), Int32.Parse(commands[5]));
                    break;
                case "StartServer":
                    startServer(commands[1], commands[2], commands[3], Int32.Parse(commands[4]), Int32.Parse(commands[5]));
                    break;
                case "GlobalStatus":
                    globalStatus();
                    break;
                case "Crash":
                    crash(commands[1]);
                    break;
                case "Freeze":
                    freeze(commands[1]);
                    break;
                case "Unfreeze":
                    unfreeze(commands[1]);
                    break;
                case "InjectDelay":
                    break;
                case "LocalState":
                    break;
                case "Wait":
                    wait(commands[1]);
                    break;
                default:
                    form.changeText("Command not found");
                    break;
            }

        }

        static void startClient(string pid, string pcs_url, string client_url, int msec_per_round, int num_players)
        {
            //IPCS = getPCS(pcs_url);

            //IPCS.create(pid, pcs_url, client_url, msec_per_round, num_players);

            pidUrl.Add(pid, client_url);
            clients.Add(client_url);

            string commands = client_url + " " + msec_per_round + " " + num_players;

            ProcessStartInfo info = new ProcessStartInfo(Client.executionPath(), commands);
            info.CreateNoWindow = false;
            Process.Start(info);
        }

        static void startServer(string pid, string pcs_url, string server_url, int msec_per_round, int num_players)
        {;
            //IPCS = getPCS(pcs_url);

            //IPCS.create(pid, pcs_url, server_url, msec_per_round, num_players);

            string commands;

            if (servers.Count == 0)
            {
                commands = server_url + " " + msec_per_round + " " + num_players + " " + 0;
            }
            else
            {
                commands = server_url + " " + msec_per_round + " " + num_players + " " + 1;
            }

            pidUrl.Add(pid, server_url);
            servers.Add(server_url);

            ProcessStartInfo info = new ProcessStartInfo(Server.executionPath(), commands);
            info.CreateNoWindow = false;
            Process.Start(info);
        }

        static void globalStatus()
        {
            string actives = "";
            string inactives = "";
            foreach(var server_url in servers)
            {
                IServer remote = RemotingServices.Connect(typeof(IServer), server_url) as IServer;
                if (remote.getStatus().Equals("On"))
                {
                    actives += "PID: " + server_url + ", ";
                }
                else
                {
                    inactives += "PID: " + server_url + ", ";
                }
            }

            foreach (var client_url in clients)
            {
                IClient remote = RemotingServices.Connect(typeof(IClient), client_url) as IClient;
                if (remote.getStatus().Equals("On"))
                {
                    actives += "PID: " + client_url + ", ";
                }
                else
                {
                    inactives += "PID: " + client_url + ", ";
                }
            }

            form.changeText("Who is alive: " + actives + "\r\n" + "Who seems to be down: " + inactives);
        }

        static void crash(string pid)
        {

            string[] words = pidUrl[pid].Split(':', '/');
            int port = Int32.Parse(words[4]);

            if (servers.Contains(pidUrl[pid]))
            {

                IServer remote = RemotingServices.Connect(typeof(IServer), "tcp://localhost:" + port + "/" + words[5]) as IServer;
                try
                {

                    pidUrl.Remove(pid);
                    remote.getProcessToCrash();
                }
                catch (Exception ex) { };
            }
            else if(clients.Contains(pidUrl[pid]))
            {
                IClient remote = RemotingServices.Connect(typeof(IClient), "tcp://localhost:" + port + "/" + words[5]) as IClient;
                try
                {
                    pidUrl.Remove(pid);
                    remote.getProcessToCrash();
                }
                catch (Exception ex) { };
            }
        }

        static void freeze(string pid)
        {

            string[] words = pidUrl[pid].Split(':', '/');
            int port = Int32.Parse(words[4]);

            if (servers.Contains(pidUrl[pid]))
            {

                IServer remote = RemotingServices.Connect(typeof(IServer), "tcp://localhost:" + port + "/" + words[5]) as IServer;
                try
                {

                    remote.freeze();
                }
                catch (Exception ex) { };
            }
            else if (clients.Contains(pidUrl[pid]))
            {
                IClient remote = RemotingServices.Connect(typeof(IClient), "tcp://localhost:" + port + "/" + words[5]) as IClient;
                try
                {
                    remote.freeze();
                }
                catch (Exception ex) { };
            }
        }

        static void unfreeze(string pid)
        {

            string[] words = pidUrl[pid].Split(':', '/');
            int port = Int32.Parse(words[4]);

            if (servers.Contains(pidUrl[pid]))
            {

                IServer remote = RemotingServices.Connect(typeof(IServer),
                "tcp://localhost:" + port + "/" + words[5]) as IServer;
                try
                {
                    remote.unfreeze();
                }
                catch (Exception ex) { };
            }
            else if (clients.Contains(pidUrl[pid]))
            {
                IClient remote = RemotingServices.Connect(typeof(IClient),
                "tcp://localhost:" + port + "/" + words[5]) as IClient;
                try
                {
                    remote.unfreeze();
                }
                catch (Exception ex) { };
            }
        }

        static void wait(string time)
        {
            System.Threading.Thread.Sleep(Int32.Parse(time));
        }

    }
}
