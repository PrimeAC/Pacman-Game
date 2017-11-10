using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace pacman {
    public partial class Form1 : Form {

        // direction player is moving in. Only one will be true
        bool goup;
        bool godown;
        bool goleft;
        bool goright;

        int boardRight = 320;
        int boardBottom = 320;
        int boardLeft = 0;
        int boardTop = 40;
        //player speed
        int speed = 5;

        int score = 0; int total_coins = 61;

        //ghost speed for the one direction ghosts
        int ghost1 = 5;
        int ghost2 = 5;
        
        //x and y directions for the bi-direccional pink ghost
        int ghost3x = 5;
        int ghost3y = 5;


        int port;
        string brodcastAddress = "255.255.255.255";
        UdpClient receivingClient;
        UdpClient sendingClient;
        Thread receivingThread;
        delegate void AddMessage(string message);


        List<string> clients = new List<string>();


        IServer server;
        
        public Form1(string gameRate) {
            port = FreeTcpPort();
            System.Console.WriteLine(port);
            TcpChannel chan = new TcpChannel(port);
            ChannelServices.RegisterChannel(chan, false);

            // Alternative 1 for service activation
            ClientServices service = new ClientServices();
            RemotingServices.Marshal(service, "Client",
                typeof(ClientServices));

           // RemotingConfiguration.RegisterWellKnownServiceType(
             //   typeof(ClientServices), "Client",
               // WellKnownObjectMode.Singleton);
            Thread.Sleep(5000);
            IServer server = (IServer)Activator.GetObject(typeof(IServer), "tcp://localhost:8086/Server");
            string gameRate = server.RegisterClient(port.ToString());
            this.server = server;
            Thread.Sleep(10000);
            foreach (IClient client in server.getClients())
            {
                if(!client.getPort().Equals(port.ToString()))
                {
                    clients.Add(client.getPort());
                }
            }

            foreach(string x in clients)
            {
                Console.WriteLine("porta: " + x);
            }

            InitializeComponent();
            label2.Visible = false;
            this.timer1.Interval = Int32.Parse(gameRate);

        }

        private void keyisdown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Left) {
                goleft = true;
                pacman.Image = Properties.Resources.Left;
            }
            if (e.KeyCode == Keys.Right) {
                goright = true;
                pacman.Image = Properties.Resources.Right;
            }
            if (e.KeyCode == Keys.Up) {
                goup = true;
                pacman.Image = Properties.Resources.Up;
            }
            if (e.KeyCode == Keys.Down) {
                godown = true;
                pacman.Image = Properties.Resources.down;
            }
            if (e.KeyCode == Keys.Enter) {
                    tbMsg.Enabled = true; tbMsg.Focus();
               }
        }

        private void keyisup(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Left) {
                goleft = false;
            }
            if (e.KeyCode == Keys.Right) {
                goright = false;
            }
            if (e.KeyCode == Keys.Up) {
                goup = false;
            }
            if (e.KeyCode == Keys.Down) {
                godown = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            label1.Text = "Score: " + score;

            //move player
            if (goleft) {
                if (pacman.Left > (boardLeft))
                    pacman.Left -= speed;
            }
            if (goright) {
                if (pacman.Left < (boardRight))
                pacman.Left += speed;
            }
            if (goup) {
                if (pacman.Top > (boardTop))
                    pacman.Top -= speed;
            }
            if (godown) {
                if (pacman.Top < (boardBottom))
                    pacman.Top += speed;
            }
            //move ghosts
            redGhost.Left += ghost1;
            yellowGhost.Left += ghost2;

            // if the red ghost hits the picture box 4 then wereverse the speed
            if (redGhost.Bounds.IntersectsWith(pictureBox1.Bounds))
                ghost1 = -ghost1;
            // if the red ghost hits the picture box 3 we reverse the speed
            else if (redGhost.Bounds.IntersectsWith(pictureBox2.Bounds))
                ghost1 = -ghost1;
            // if the yellow ghost hits the picture box 1 then wereverse the speed
            if (yellowGhost.Bounds.IntersectsWith(pictureBox3.Bounds))
                ghost2 = -ghost2;
            // if the yellow chost hits the picture box 2 then wereverse the speed
            else if (yellowGhost.Bounds.IntersectsWith(pictureBox4.Bounds))
                ghost2 = -ghost2;
            //moving ghosts and bumping with the walls end
            //for loop to check walls, ghosts and points
            foreach (Control x in this.Controls) {
                // checking if the player hits the wall or the ghost, then game is over
                if (x is PictureBox && x.Tag == "wall" || x.Tag == "ghost") {
                    if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds)) {
                        pacman.Left = 0;
                        pacman.Top = 25;
                        label2.Text = "GAME OVER";
                        label2.Visible = true;
                        timer1.Stop();
                    }
                }
                if (x is PictureBox && x.Tag == "coin") {
                    if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds)) {
                        this.Controls.Remove(x);
                        score++;
                        //TODO check if all coins where "eaten"
                        if (score == total_coins) {
                            //pacman.Left = 0;
                            //pacman.Top = 25;
                            label2.Text = "GAME WON!";
                            label2.Visible = true;
                            timer1.Stop();
                            }
                    }
                }
            }
                pinkGhost.Left += ghost3x;
                pinkGhost.Top += ghost3y;

                if (pinkGhost.Left < boardLeft ||
                    pinkGhost.Left > boardRight ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox1.Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox2.Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox3.Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox4.Bounds))) {
                    ghost3x = -ghost3x;
                }
                if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2) {
                    ghost3y = -ghost3y;
                }
        }

        private void tbMsg_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {

                string toSend = port + ": " + tbMsg.Text;
                byte[] data = Encoding.ASCII.GetBytes(toSend);
                sendingClient.Send(data, data.Length);
                tbChat.Text += "\r\n" + port + ": " + tbMsg.Text;
                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }

        private void Receiver()
        {

            if (clients != null)
            {
                foreach (string portToConnect in clients)
                {
                    
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Int32.Parse(portToConnect));
                    AddMessage messageDelegate = MessageReceived;

                    while (true)
                    {
                        byte[] data = receivingClient.Receive(ref endPoint);
                        string message = Encoding.ASCII.GetString(data);
                        Invoke(messageDelegate, message);
                    }

                }

            }
        }

        private void MessageReceived(string message)
        {
            tbChat.Text += "\r\n" + message;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //InitializeSender();
            //InitializeReceiver();
        }

        private void InitializeSender(){

            sendingClient = new UdpClient(brodcastAddress, port);
            sendingClient.EnableBroadcast = true;
        }

        private void InitializeReceiver()
        {
            if (clients != null)
            {
                foreach (string portToConnect in clients)
                {
                    receivingClient = new UdpClient(Int32.Parse(portToConnect));
                    
                    ThreadStart start = new ThreadStart(Receiver);
                    receivingThread = new Thread(start);
                    receivingThread.IsBackground = true;
                    receivingThread.Start();

                }
            }
            
        }



        public class ClientServices : MarshalByRefObject, IClient
        {
            public string port;

            public ClientServices()
            {
            }

            public void setPort(string port)
            {
                this.port = port;
            }

            public string getPort()
            {
                return port;
            }

            public void startGame()
            {
                
            }
        }
    }
}
