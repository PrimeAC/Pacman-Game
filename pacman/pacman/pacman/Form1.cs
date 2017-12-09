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
        private Dictionary<int[], string[]> onHold = new Dictionary<int[], string[]>();

        //saves all messages that have been displayed
        private Dictionary<int[], string> displayed = new Dictionary<int[], string>();

        //saves the order of the displayed messages
        private List<int[]> messageorder = new List<int[]>();

        private System.Windows.Forms.Timer timer2;

        private List<IClient> clients = new List<IClient>();

        static object _lockform = new Object();

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
                clients.Add(client);
            }

            //to simulate that a message 2 arrives first that a message 1
            //only works for two clients
            //if there were three the aux should have 3 fields
            int[] aux = { 1, 2 };
            string[] saux = { "1111: teste", "12/5/2017 19:03:06" };
            onHold.Add(aux, saux);
            Console.WriteLine("on hold {0}, {1}, mensagem {2}", aux[0], aux[1], onHold[aux]);
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

            // timer2
            // 
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.timer2.Enabled = true;
            this.timer2.Interval = 30000;  //each 30 seconds check if there are messages that could have been lost
            this.timer2.Tick += new System.EventHandler(this.waitingMessages);

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
                        server.gameOver(ip+":"+port);
                    }
                    else if(pacman.Value[2] == -2)
                    {
                        label2.Text = "GAME WON!";
                        label2.Visible = true;
                        timer1.Stop();
                        server.gameOver(ip + ":" + port);
                    }
                    else
                    {
                        label1.Text = "Score"+ id +": " + pacmans[pacman.Key][2];
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
        }

       
        private void tbMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                vector[id] += 1;
               
                Console.WriteLine("vector: [{0}, {1}]", vector[0], vector[1]);
                if(id == 1 && vector[id] == 1)
                {
                    AddMsg(tbMsg.Text, vector);
                }
                else
                {
                    client2.SendMsg(port + ": " + tbMsg.Text, vector);
                }
                
                tbMsg.Clear();
                tbMsg.Enabled = false;
                this.Focus();
            }
        }


        public void AddMsg(string s, int[] vetor)
        {
            Console.WriteLine("vetor do add {0}, {1}, displayed: {2}, on hold {3} ", vetor[0], vetor[1], displayed.ContainsKey(vetor), onHold.ContainsKey(vector));
            int flag = 0;     //if flag < (vector.count - 1) means that one or more messages are missing
            int[] aux = new int[vector.Length];
            bool flag1 = false;
            if (displayed.Count != 0)
            {
                foreach (KeyValuePair<int[], string> pair in displayed)
                {
                    Console.WriteLine("flag1 dentro " + flag1);
                    flag1 = true;
                    for (int i = 0; i < vetor.Length; i++)
                    {
                        if (vetor[i] != pair.Key[i])
                        {
                            Console.WriteLine("e diferente");
                            flag1 = false;
                            break;
                        }
                    }
                    Console.WriteLine("valores par {0}, {1}, valores vector {2}, {3}", pair.Key[0], pair.Key[1], vetor[0], vetor[1]);
                    if (flag1)
                    {
                        Console.WriteLine("entrie dentro do if e fiz break");
                        break;
                    }
                }
            }
            Console.WriteLine("flag1 " + flag1);
            if (!flag1)
            //if (!displayed.ContainsKey(vetor))
            //bool display = isDisplayed(vetor);
            //if(!display)
            {
                Console.WriteLine("ENTREI no primeiro");
                if (!onHold.ContainsKey(vetor))
                {
                    Console.WriteLine("ENTREI no segundo");
                    for (int i = 0; i < this.vector.Length; i++)
                    {
                        if (this.vector[i] == vetor[i])
                        {
                            flag++;
                        }
                        if (this.vector[i] + 1 < vetor[i])
                        {
                            flag = 0;
                            break;
                        }
                        if (this.vector[i] > vetor[i] + 1)
                        {
                            //special case, only appens when a message arrives late from another client
                            flag = -1;
                            break;
                        }
                    }
                    if (flag >= this.vector.Length - 1)
                    {
                        //means that it's receiving a valid message
                        this.tbChat.AppendText("\r\n" + s);
                        lock (_lockform)
                        {
                            Console.WriteLine("TENHO : {0}, {1}", vector[0], vector[1]);
                            this.vector = vetor;
                            Console.WriteLine("A ADICIONAR : {0}, {1}", vector[0], vector[1]);
                            //getDisplay(vector);
                            for(int i = 0; i < aux.Length; i++)
                            {
                                aux[i] = vector[i];
                            }
                            displayed.Add(aux, s);
                            getDisplay();
                            //seeOrder();
                            messageorder.Add(aux);
                            seeOrder();
                        }
                        isWaitting();  //check if there are any messages waitting dependent on this
                    }
                    else if (flag == -1)
                    {
                        this.tbChat.AppendText("\r\n" + s);
                        Console.WriteLine("A ADICIONAR especial : {0}, {1}", vetor[0], vetor[1]);
                        lock (_lockform)
                        {
                            for (int i = 0; i < aux.Length; i++)
                            {
                                aux[i] = vetor[i];
                            }
                            displayed.Add(aux, s);
                        }
                    }
                    else
                    {
                        string[] saux = { s, DateTime.Now.ToString() };
                        for (int i = 0; i < aux.Length; i++)
                        {
                            aux[i] = vetor[i];
                        }
                        Console.WriteLine("on hold " + aux[0] + " - " + aux[1]);
                        onHold.Add(aux, saux);
                    }
                }
            }
            //getDisplay();
        }

        public void isWaitting()
        {
            int counter = 0; //if counter != vector.len - 1 means that the message is still deppendent on other
            int[] aux = new int[vector.Length];
            string[] temp = new string[2];
            foreach (KeyValuePair<int[], string[]> pair in onHold)
            {
                counter = 0;
                if (vector.Length == 2)
                {
                    Console.WriteLine("len 2");
                    for (int i = 0; i < vector.Length; i++)
                    {
                        Console.WriteLine("comparaçao {0} {1}" ,pair.Key[i], vector[i]);
                        if (pair.Key[i] > vector[i] + 1)
                        {
                            counter = 0;
                            break;
                        }
                        else if(pair.Key[i] < vector[i] + 1)
                        {
                            counter++;
                        }
                    }
                    if (counter == 1)
                    {
                        Console.WriteLine("counter é 1");
                        vector = pair.Key;
                        aux = vector;
                        temp = onHold[pair.Key];
                        break;
                    }
                }
                else if (vector.Length > 2)
                {
                    for (int i = 0; i < vector.Length; i++)
                    {
                        if (pair.Key[i] > vector[i] + 1)
                        {
                            break;
                        }
                        else if (pair.Key[i] <= vector[i])
                        {
                            counter++;
                        }
                    }
                    if (counter == vector.Length - 1)
                    {
                        vector = pair.Key;
                        aux = vector;
                        temp = onHold[pair.Key];
                        break;
                    }

                }
                
            }
            if (aux == vector)
            {
                Console.WriteLine("uma mensagem a espera que pode ser enviada");
                onHold.Remove(vector);
                AddMsg(temp[0], vector);
            }
        }

        public void waitingMessages(object sender, EventArgs e)
        {
            foreach (KeyValuePair<int[], string[]> pair in onHold)
            {
                Console.WriteLine("data on hold: {0}, data atual: {1} chave [{2}, {3}]", DateTime.Parse(onHold[pair.Key][1]), DateTime.Now, pair.Key[0], pair.Key[1]);
                if (DateTime.Parse(onHold[pair.Key][1]).AddMinutes(1) < DateTime.Now)
                {
                    //means that the message is waiting for at least for a minute
                    //possibly the message in who is depending was lost
                    //it is needed to ask for it again
                    Console.WriteLine("dentro do if -> data on hold: {0}, data atual: {1}, à procura da chave [{2}, {3}]", DateTime.Parse(onHold[pair.Key][1]), DateTime.Now, pair.Key[0], pair.Key[1]);
                    client2.requestMessage(pair.Key);
                }
            }
        }

        //public Boolean isDisplayed(int[] vector)
        //{
        //    bool flag = true;
        //    foreach (KeyValuePair<int[], string> pair in displayed)
        //    {
        //        flag = true;
        //        for (int i = 0; i < vector.Length; i++)
        //        {
        //            Console.WriteLine("valores par {0}, {1}, valores vector {2}, {3}", pair.Key[0], pair.Key[1], vector[0], vector[1]);
        //            if (vector[i] != pair.Key[i])
        //            {
        //                Console.WriteLine("e diferente");
        //                flag = false;
        //                break;
        //            }
        //        }
        //        if (flag)
        //        {
        //            Console.WriteLine("encontrei");
        //            return flag;
        //        }
        //    }
        //    return flag;
        //}

        public void gotVector(int[] vector)
        {
            Console.WriteLine("vou procurar [{0}, {1}], {2} {3} ", vector[0], vector[1], displayed.ContainsKey(vector), displayed.ContainsKey(vector));
            getDisplay();
            bool flag = false;
            foreach (KeyValuePair<int[], string> pair in displayed)
            {
                flag = true;
                for (int i = 0; i < vector.Length; i++)
                {
                    if (vector[i] != pair.Key[i])
                    {
                        Console.WriteLine("e diferente");
                        flag = false;
                        break;
                    }
                }
                Console.WriteLine("valores par {0}, {1}, valores vector {2}, {3}", pair.Key[0], pair.Key[1], vector[0], vector[1]);
                if (flag)
                {
                    break;
                }
            }
            
            if(flag)
            //if (displayed.ContainsKey(vector))
            //if (isDisplayed(vector))
            {
                Console.WriteLine("tenho a chave que procuras " + vector);
                //this client haves the message displayed, that means that haves the one that other messages are depending
                for (int i = 0; i < messageorder.Count; i++)
                {
                    int cnt1 = 0;
                    for (int j = 0; j < vector.Length; j++)
                    {
                        if (messageorder[i][j] == vector[j])
                        {
                            cnt1++;
                        }
                    }
                    if (cnt1 == vector.Length)
                    {
                        Console.WriteLine("vou enviar {0} - {1}, i = {2}", displayed[messageorder[i - 1]], messageorder[i - 1], i);
                        client2.SendMsg(displayed[messageorder[i - 1]], messageorder[i - 1]);
                        break;
                    }
                }
            }
        }

        public void getDisplay()
        {
            Console.WriteLine("get do tamanho do displayed: {0}", displayed.Count);
            foreach (KeyValuePair<int[], string> pair in displayed)
            {
                Console.WriteLine("displayed {0}, {1}, {2}", displayed[pair.Key], pair.Key[0], pair.Key[1]);
            }
        }

        public void seeOrder()
        {
            for (int i = 0; i < messageorder.Count; i++)
            {
                Console.WriteLine("ordem [{0}, {1}], valor do i {2}", messageorder[i][0], messageorder[i][1], i);
            }
        }

        public List<IClient> getClients()
        {
            return clients;
        }
    }
}
