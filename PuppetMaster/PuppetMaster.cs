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

        //static IServer server;
        //static IPCS IPCS;
        static PuppetMasterWindow form;
        //private static Dictionary<string, IPCS> pcs = new Dictionary<string, IPCS>();
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

        private static void consoleApp()
        {
            string input;
            while (true)
            {
                input = Console.ReadLine();
                //readConsole(input, 1);
            };
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
                    break;
                case "Crash":
                    break;
                case "Freeze":
                    break;
                case "Unfreeze":
                    break;
                case "InjectDelay":
                    break;
                case "LocalState":
                    break;
                case "Wait":
                    break;
                default:
                    form.changeText("Command not found");
                    break;
            }

        }

        static void startClient(string pid, string pcs_url, string client_url, int msec_per_round, int num_players)
        {
            //Clients.Add(client_url);
            //pidUrl.Add(pid, client_url);

            //IPCS = getPCS(pcs_url);

            //IPCS.create(pid, pcs_url, client_url, msec_per_round, num_players);

            pidUrl.Add(pid, client_url);
            clients.Add(client_url);

            string commands = client_url + " " + msec_per_round + " " + num_players;
            Console.WriteLine(commands);
            ProcessStartInfo info = new ProcessStartInfo(Client.executionPath(), commands);
            info.CreateNoWindow = false;
            Process.Start(info);
        }

        static void startServer(string pid, string pcs_url, string server_url, int msec_per_round, int num_players)
        {
            //Server.Add(server_url);
            //pidUrl.Add(pid, server_url);

            //IPCS = getPCS(pcs_url);

            //IPCS.create(pid, pcs_url, server_url, msec_per_round, num_players);

            pidUrl.Add(pid, server_url);
            servers.Add(server_url);

            string commands = server_url + " " + msec_per_round + " " + num_players;

            ProcessStartInfo info = new ProcessStartInfo(Server.executionPath(), commands);
            info.CreateNoWindow = false;
            Process.Start(info);
        }



    }
}
