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
        private int id = 0;  //saves the position of the client in the pacmans list

        int port;
        string ip;

        delegate void AddMessage(string message);


        private int[] vector;   //list used to assure causal order in the messages

        //saves all messages that are waiting to be displayed
        private Dictionary<int[], string> onHold = new Dictionary<int[], string>();

        IServer server;
        IClient client2;

        public Form1(string gameRate, string numPlayers, IServer server, string ip, int port, IClient client1) {
            
            ClientServices.form = this;
            this.ip = ip;
            this.port = port;
            this.server = server;
            this.client2 = client1;
            this.array = new PictureBox[Int32.Parse(numPlayers)];
            this.vector = new int[Int32.Parse(numPlayers)];
            cnt = 0;
            foreach (IClient client in server.getClients())
            {
                if (client.getPort().Equals(port.ToString()) && client.getIP().Equals(ip))
                {
                    this.id = cnt;
                }
                vector[cnt] = 0;    //initializes the list with all positions equal to zero
                cnt++;
            }

            //to simulate that a message 2 arrives first that a message 1
            //only works for two clients
            //if there were three the aux should have 3 fields
            //int[] aux = { 1, 2 };
            //onHold.Add(aux, "1111: teste");
            //Console.WriteLine("on hold {0}, {1}, mensagem {2}", aux[0], aux[1], onHold[aux]);
            //int[] aux1 = { 1, 3 };
            //onHold.Add(aux1, "1111: teste recursivo");
            //Console.WriteLine("on hold {0}, {1}, mensagem {2}", aux1[0], aux1[1], onHold[aux1]);
            //int[] aux2 = { 3, 4 };
            //onHold.Add(aux2, "1111: teste entre espaços");
            //Console.WriteLine("on hold {0}, {1}, mensagem {2}", aux2[0], aux2[1], onHold[aux2]);

            //to simulate that a message 2 arrives first that a message 1
            //only works for three clients
            //int[] aux = { 1, 2, 1 };
            //onHold.Add(aux, "1111: teste");
            //Console.WriteLine("on hold {0}, {1}, mensagem {2}", aux[0], aux[1], onHold[aux]);
            //int[] aux1 = { 1, 3 , 1};
            //onHold.Add(aux1, "1111: teste recursivo");
            //Console.WriteLine("on hold {0}, {1}, mensagem {2}", aux1[0], aux1[1], onHold[aux1]);
            //int[] aux2 = { 3, 4, 2 };
            //onHold.Add(aux2, "1111: teste entre espaços");
            //Console.WriteLine("on hold {0}, {1}, mensagem {2}", aux2[0], aux2[1], onHold[aux2]);



            InitializeComponent();
            label2.Visible = false;
            this.timer1.Interval = Int32.Parse(gameRate);

            List<string> messages = client2.getMessages();
            foreach (object o in messages)
            {
                AddMsg((string)o, vector);
            }

            server.readyClient();
            
        }

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
            //foreach(KeyValuePair<string, int[]> pacman in pacmans)
            //{
            //    Console.WriteLine("value 1: {0}, value 2: {1}, value 3: {2}", pacmans[pacman.Key][0], pacmans[pacman.Key][1], pacmans[pacman.Key][2]);
            //}
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
                                removecoin = 0;  //means that don't need to remove this coin
                            }
                        }
                        if(removecoin == 1)
                        {
                            //means that th coin doesn't exist anymore in the coins dictionary
                            //so it has to be removed
                            this.Controls.Remove(x);
                        }
                    }
                }
            }
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
                vector[id] += 1;
               
                Console.WriteLine("vector: [{0}, {1}]", vector[0], vector[1]);

                client2.SendMsg(port + ": " + tbMsg.Text, vector);
                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }


        public void AddMsg(string s, int[] vetor)
        {
            int flag = 0;     //if flag < (vector.count - 1) means that one or more messages are missing
            Console.WriteLine("RECEBI [{0}, {1}, {2}]", vetor[0], vetor[1], vetor[2]);
            Console.WriteLine("TENHO [{0}, {1}, {2}]", this.vector[0], this.vector[1], this.vector[2]);
            for(int i = 0; i < this.vector.Length; i++)
            {
                if(this.vector[i] == vetor[i])
                {
                    flag++;
                }
            }
            if(flag >= this.vector.Length - 1)
            {
                //means that it's receiving a valid message
                this.tbChat.AppendText("\r\n" + s);
                this.vector = vetor;
                Console.WriteLine("vetor alterado [{0}, {1}]", this.vector[0], this.vector[1]);
                isWaitting();  //check if there are any messages waitting dependent on this
            }
            else
            {
                onHold.Add(vetor, s); 
            }  
        }

        public void isWaitting()
        {
            int counter = 0; //if counter != vector.len - 1 means that the message is still deppendent on other
            int j = 0;
            bool found = false;
            int[] aux = new int[vector.Length];
            int[] aux1 = new int[vector.Length];
            string temp = "";
            foreach (KeyValuePair<int[], string> pair in onHold)
            {
                Console.WriteLine("pair: {0}, pair[0] {1}, pair[1] {2}, pair[2] {3}", pair.Key, pair.Key[0], pair.Key[1], pair.Key[2]);
                //if(((pair.Key[0] == this.vector[0] + 1) && (pair.Key[1] == this.vector[1])) || ((pair.Key[0] == this.vector[0]) && (pair.Key[1] == this.vector[1] + 1)))
                //{   //means that exists one message waiting to be written
                //    Console.WriteLine("entrei ");
                //    this.vector[0] = pair.Key[0];
                //    this.vector[1] = pair.Key[1];
                //    aux = vector;
                //    aux1 = pair.Key;
                //    temp = onHold[pair.Key];
                //    break;
                //}

                counter = 0;
                if (vector.Length == 2)
                {
                    Console.WriteLine("entrei no comprimento dois");
                    for (int i = 0; i < vector.Length; i++)
                    {
                        if (pair.Key[i] > vector[i] + 1)
                        {
                            break;
                        }
                        else if(pair.Key[i] < vector[i] + 1)
                        {
                            counter++;
                            j = i;
                            Console.WriteLine("counter " + counter);
                        }
                    }
                    if (counter == 1)
                    {
                        Console.WriteLine("entrei " + pair.Key[j]);
                        vector = pair.Key;
                        aux = vector;
                        aux1 = pair.Key;
                        temp = onHold[pair.Key];
                        found = true;
                        break;
                    }
                }
                else if (vector.Length > 2)
                {
                    Console.WriteLine("entrei no comprimento tres");
                    for (int i = 0; i < vector.Length; i++)
                    {
                        Console.WriteLine("comparaçao " + pair.Key[i] + " < " + (vector[i] + 1));
                        if (pair.Key[i] > vector[i] + 1)
                        {
                            //found = true;
                            break;
                        }
                        else if (pair.Key[i] <= vector[i])
                        {
                            counter++;
                            //j = i;
                            Console.WriteLine("counter " + counter);
                            //vector[i] = pair.Key[i];
                            //aux = vector;
                            //aux1 = pair.Key;
                            //temp = onHold[pair.Key];
                            //found = true;
                            //break;
                        }
                        else
                        {
                            j = i;
                        }
                    }
                    if (counter == vector.Length - 1)
                    {
                        Console.WriteLine("entrei " + pair.Key[j]);
                        vector = pair.Key;
                        aux = vector;
                        aux1 = pair.Key;
                        temp = onHold[pair.Key];
                        found = true;
                        break;
                    }

                }
                
                //if (found)
                //{
                //    break;
                //}
            }
            if (aux == vector)
            {
                Console.WriteLine("ESTE " + onHold[aux1] + " -- " + temp + "  " + vector[0]+"," + vector[1] + "," + vector[2]);
                onHold.Remove(aux1);
                AddMsg(temp, vector);
            }
        }


        public IServer getServer()
        {
            return this.server;
        }


    }
}
