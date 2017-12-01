using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Runtime;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using RemoteServices;
using System.Net;
using static pacman.Client;
using System.Resources;

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

        private System.Windows.Forms.PictureBox[] array;
        private int cnt = 0;
        private int removecoin = 1;  //if equals 1 remove the coin from controls
        private int id;  //saves the position of the client in the pacmans list

        int port;
        string ip;

        delegate void AddMessage(string message);


        List<string> clients = new List<string>();

        IServer server;
        IClient client2;

        public Form1(string gameRate, string numPlayers, IServer server, string ip, int port, IClient client1) {
            
            ClientServices.form = this;
            this.ip = ip;
            this.port = port;
            this.server = server;
            this.client2 = client1;
            this.array = new PictureBox[Int32.Parse(numPlayers)];
            cnt = 0;
            foreach (IClient client in server.getClients())
            {
                if(!client.getPort().Equals(port.ToString()) && !client.getIP().Equals(ip))
                {
                    clients.Add(client.getIP() + ":" + client.getPort());
                }
                else
                {
                    this.id = cnt;
                }
                cnt++;
            }
            

            InitializeComponent();
            label2.Visible = false;
            this.timer1.Interval = Int32.Parse(gameRate);
            //playersInit(Int32.Parse(numPlayers));

            List<string> messages = client2.getMessages();
            foreach (object o in messages)
            {
                AddMsg((string)o);
            }

            server.readyClient();
            
        }

        //private void playersInit(int numPlayers)
        //{
        //    if (numPlayers == 1)
        //    {
        //        this.pacman2.Visible = false;
        //        this.pacman3.Enabled = false;
        //        this.pacman4.Enabled = false;
        //        this.pacman5.Enabled = false;
        //       this.pacman6.Enabled = false;
        //    }
        //    else if (numPlayers <= 2)
        //    {
        //        this.pacman3.Enabled = false;
        //        this.pacman4.Enabled = false;
        //        this.pacman5.Enabled = false;
        //        this.pacman6.Enabled = false;
        //    }
        //    else if (numPlayers <= 3)
        //    {
        //        this.pacman4.Enabled = false;
        //        this.pacman5.Enabled = false;
        //        this.pacman6.Enabled = false;
        //    }
        //    else if (numPlayers <= 4)
        //    {
        //        this.pacman5.Enabled = false;
        //        this.pacman6.Enabled = false;
        //    }
        //    else if (numPlayers <= 5)
        //    {
        //        this.pacman6.Enabled = false;
        //    }
        //}

        private void keyisdown(object sender, KeyEventArgs e) { 
            if (e.KeyCode == Keys.Left) {
                goleft = true;
                //pacman.Image = Properties.Resources.Left;
                array[id].Image = Properties.Resources.Left;
            }
            if (e.KeyCode == Keys.Right) {
                goright = true;
                //pacman.Image = Properties.Resources.Right;
                array[id].Image = Properties.Resources.Right;
            }
            if (e.KeyCode == Keys.Up) {
                goup = true;
                //pacman.Image = Properties.Resources.Up;
                array[id].Image = Properties.Resources.Up;
            }
            if (e.KeyCode == Keys.Down) {
                godown = true;
                //pacman.Image = Properties.Resources.down;
                array[id].Image = Properties.Resources.down;
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

        //problema de quando jogar com 2 ou mais jogadores ele faz um movimento sempre na mesma direçao
        public void updateGame(Dictionary<string, int[]> pacmans, Dictionary<int, int[]> ghosts, Dictionary<int, int[]> coins)
        {
            //move ghosts
            foreach(KeyValuePair<int, int[]> ghost in ghosts)
            {
                if (ghost.Key == 1)
                {
                    System.Drawing.Point point = new System.Drawing.Point(ghosts[ghost.Key][0], ghosts[ghost.Key][1]);
                    this.pinkGhost.Location = point;
                }
                else if(ghost.Key == 2)
                {
                    this.yellowGhost.Location = new System.Drawing.Point(ghosts[ghost.Key][0], ghosts[ghost.Key][1]);
                }
                else if (ghost.Key == 3)
                {
                    this.redGhost.Location = new System.Drawing.Point(ghosts[ghost.Key][0], ghosts[ghost.Key][1]);
                }
            }

            //move pacmans
            cnt = 0;
            foreach (KeyValuePair<string, int[]> pacman in pacmans)
            {
                if (pacman.Key.Equals(ip + ":" + port))
                {
                    Console.WriteLine("sou eu " + pacman.Value[2]);
                    if(pacman.Value[2] == -1)
                    {
                        label2.Text = "GAME OVER";
                        label2.Visible = true;
                        timer1.Stop();
                    }
                    else if(pacman.Value[2] == -2)
                    {
                        label2.Text = "GAME WON!";
                        label2.Visible = true;
                        timer1.Stop();
                    }
                    else
                    {
                        label1.Text = "Score: " + pacmans[pacman.Key][2];
                    }
                }
                array[cnt++].Location = new System.Drawing.Point(pacmans[pacman.Key][0], pacmans[pacman.Key][1]);
                foreach (Control x in this.Controls)
                {
                    if (x is PictureBox && x.Tag == "coin")
                    {
                        removecoin = 1;
                        foreach(KeyValuePair<int, int[]> coin in coins)
                        {
                            if (x.Location == new Point(coin.Value[0], coin.Value[1]))
                            {
                                Console.WriteLine("encontrei uma location existente");
                                removecoin = 0;  //means that don't need to remove this coin
                            }
                        }
                        if(removecoin == 1)
                        {
                            //means that th coin doesn't exist anymore in the coins dictionary
                            //so it has to be removed
                            this.Controls.Remove(x);
                            Console.WriteLine("a remover moeda");

                        }
                    }
                }
            }

            //if (mov.Equals("left"))
            //{
            //    goleft = true;
            //}

            //if (mov.Equals("right"))
            //{
            //    goright = true;
            //}
            //if (mov.Equals("up"))
            //{
            //    goup = true;
            //}
            //if (mov.Equals("down"))
            //{
            //    godown = true;
            //}


        }

        public void initializeGame(Dictionary<string, int[]> pacmans)
        {
            cnt = 0;
            foreach (KeyValuePair<string,int[]> pacman in pacmans)
            {
                array[cnt] = new PictureBox {
                    BackColor = System.Drawing.Color.Transparent,
                    Location = new System.Drawing.Point(pacmans[pacman.Key][0], pacmans[pacman.Key][1]),
                    Name = pacman.Key,  //creates a pacman with its ip:port as name
                    Size = new System.Drawing.Size(25, 25),
                    SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage,
                    Image = global::pacman.Properties.Resources.Left,
                    Tag = "pacman"
                };
                this.Controls.Add(array[cnt++]);
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (goleft)
            {
                server.sendMove(ip, port.ToString(), "left");
            }
            if (goright)
            {
                server.sendMove(ip, port.ToString(), "right");
            }
            if (goup)
            {
                server.sendMove(ip, port.ToString(), "up");
            }
            if (godown)
            {
                server.sendMove(ip, port.ToString(), "down");
            }

            //move player
            //if (goleft)
            //{
            //    if (pacman.Left > (boardLeft))
            //        pacman.Left -= speed;
            //}
            //if (goright)
            //{
            //    if (pacman.Left < (boardRight))
            //        pacman.Left += speed;
            //}
            //if (goup)
            //{
            //    if (pacman.Top > (boardTop))
            //        pacman.Top -= speed;
            //}
            //if (godown)
            //{
            //    if (pacman.Top < (boardBottom))
            //        pacman.Top += speed;

            //    godown = false;
            //}


            ////move ghosts
            //redGhost.Left += ghost1;
            //yellowGhost.Left += ghost2;

            //// if the red ghost hits the picture box 4 then wereverse the speed
            //if (redGhost.Bounds.IntersectsWith(pictureBox1.Bounds))
            //    ghost1 = -ghost1;
            //// if the red ghost hits the picture box 3 we reverse the speed
            //else if (redGhost.Bounds.IntersectsWith(pictureBox2.Bounds))
            //    ghost1 = -ghost1;
            //// if the yellow ghost hits the picture box 1 then wereverse the speed
            //if (yellowGhost.Bounds.IntersectsWith(pictureBox3.Bounds))
            //    ghost2 = -ghost2;
            //// if the yellow chost hits the picture box 2 then wereverse the speed
            //else if (yellowGhost.Bounds.IntersectsWith(pictureBox4.Bounds))
            //    ghost2 = -ghost2;
            ////moving ghosts and bumping with the walls end
            ////for loop to check walls, ghosts and points
            //foreach (Control x in this.Controls)
            //{
            //    // checking if the player hits the wall or the ghost, then game is over
            //    if (x is PictureBox && x.Tag == "wall" || x.Tag == "ghost")
            //    {
            //        if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds))
            //        {
            //            pacman.Left = 0;
            //            pacman.Top = 25;
            //            label2.Text = "GAME OVER";
            //            label2.Visible = true;
            //            timer1.Stop();
            //        }
            //    }
            //    if (x is PictureBox && x.Tag == "coin")
            //    {
            //        if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds))
            //        {
            //            this.Controls.Remove(x);
            //            score++;
            //            //TODO check if all coins where "eaten"
            //            if (score == total_coins)
            //            {
            //                //pacman.Left = 0;
            //                //pacman.Top = 25;
            //                label2.Text = "GAME WON!";
            //                label2.Visible = true;
            //                timer1.Stop();
            //            }
            //        }
            //    }
            //}
            //pinkGhost.Left += ghost3x;
            //pinkGhost.Top += ghost3y;

            //if (pinkGhost.Left < boardLeft ||
            //    pinkGhost.Left > boardRight ||
            //    (pinkGhost.Bounds.IntersectsWith(pictureBox1.Bounds)) ||
            //    (pinkGhost.Bounds.IntersectsWith(pictureBox2.Bounds)) ||
            //    (pinkGhost.Bounds.IntersectsWith(pictureBox3.Bounds)) ||
            //    (pinkGhost.Bounds.IntersectsWith(pictureBox4.Bounds)))
            //{
            //    ghost3x = -ghost3x;
            //}
            //if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2)
            //{
            //    ghost3y = -ghost3y;
            //}

        }

        //private void timer1_Tick(object sender, EventArgs e) {
        //    label1.Text = "Score: " + score;


        //    //move player
        //    if (goleft) {
        //        server.sendMove(port.ToString(), "left");
        //        if (pacman1.Left > (boardLeft))
        //            pacman1.Left -= speed;
        //    }
        //    if (goright) {
        //        server.sendMove(port.ToString(), "right");
        //        if (pacman1.Left < (boardRight))
        //        pacman1.Left += speed;
        //    }
        //    if (goup) {
        //        server.sendMove(port.ToString(), "up");
        //        if (pacman1.Top > (boardTop))
        //            pacman1.Top -= speed;
        //    }
        //    if (godown) {
        //        server.sendMove(port.ToString(), "down");
        //        if (pacman1.Top < (boardBottom))
        //            pacman1.Top += speed;
        //    }
        //    //move ghosts
        //    redGhost.Left += ghost1;
        //    yellowGhost.Left += ghost2;

        //    // if the red ghost hits the picture box 4 then wereverse the speed
        //    if (redGhost.Bounds.IntersectsWith(pictureBox1.Bounds))
        //        ghost1 = -ghost1;
        //    // if the red ghost hits the picture box 3 we reverse the speed
        //    else if (redGhost.Bounds.IntersectsWith(pictureBox2.Bounds))
        //        ghost1 = -ghost1;
        //    // if the yellow ghost hits the picture box 1 then wereverse the speed
        //    if (yellowGhost.Bounds.IntersectsWith(pictureBox3.Bounds))
        //        ghost2 = -ghost2;
        //    // if the yellow chost hits the picture box 2 then wereverse the speed
        //    else if (yellowGhost.Bounds.IntersectsWith(pictureBox4.Bounds))
        //        ghost2 = -ghost2;
        //    //moving ghosts and bumping with the walls end
        //    //for loop to check walls, ghosts and points
        //    foreach (Control x in this.Controls) {
        //        // checking if the player hits the wall or the ghost, then game is over
        //        if (x is PictureBox && x.Tag == "wall" || x.Tag == "ghost") {
        //            if (((PictureBox)x).Bounds.IntersectsWith(pacman1.Bounds)) {
        //                pacman1.Left = 0;
        //                pacman1.Top = 25;
        //                label2.Text = "GAME OVER";
        //                label2.Visible = true;
        //                timer1.Stop();
        //            }
        //        }
        //        if (x is PictureBox && x.Tag == "coin") {
        //            if (((PictureBox)x).Bounds.IntersectsWith(pacman1.Bounds)) {
        //                this.Controls.Remove(x);
        //                score++;
        //                //TODO check if all coins where "eaten"
        //                if (score == total_coins) {
        //                    //pacman.Left = 0;
        //                    //pacman.Top = 25;
        //                    label2.Text = "GAME WON!";
        //                    label2.Visible = true;
        //                    timer1.Stop();
        //                    }
        //            }
        //        }
        //    }
        //        pinkGhost.Left += ghost3x;
        //        pinkGhost.Top += ghost3y;

        //        if (pinkGhost.Left < boardLeft ||
        //            pinkGhost.Left > boardRight ||
        //            (pinkGhost.Bounds.IntersectsWith(pictureBox1.Bounds)) ||
        //            (pinkGhost.Bounds.IntersectsWith(pictureBox2.Bounds)) ||
        //            (pinkGhost.Bounds.IntersectsWith(pictureBox3.Bounds)) ||
        //            (pinkGhost.Bounds.IntersectsWith(pictureBox4.Bounds))) {
        //            ghost3x = -ghost3x;
        //        }
        //        if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2) {
        //            ghost3y = -ghost3y;
        //        }
        //}

        private void tbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                client2.SendMsg(port + ": " + tbMsg.Text);
                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }


        public void AddMsg(string s)
        {
            this.tbChat.AppendText("\r\n" + s);
        }

        public IServer getServer()
        {
            return this.server;
        }


    }
}
