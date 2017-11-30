using RemoteServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Server
{
    class GameEngine
    {
        //saves all coins positions i, [x, y]
        public static Dictionary<int, int[]> coins = new Dictionary<int, int[]>();
        //saves all walls positions i, [x, y]
        public static Dictionary<int, int[]> walls = new Dictionary<int, int[]>();
        //saves all ghosts positions i, [x, y]
        public static Dictionary<int, int[]> ghosts = new Dictionary<int, int[]>();
        //saves all pacmans ip:port, [x, y, score]
        public static Dictionary<string, int[]> pacmans = new Dictionary<string, int[]>();
        //saves all the moves from one round ip:port, move
        public static Dictionary<string, string> moves = new Dictionary<string, string>();
        //saves all pacmans ip:port, score
        //public static Dictionary<string, int> score = new Dictionary<string, int>();

        private int number = 0;
        private int width = 328;
        private int heigth = 320;
        private int pacmansize = 25;
        private int ghostsize = 30;
        private int wallWidth = 15;
        private int coinsize = 15;

        private int top = 40;
        private int bottom = 320;
        private int left = 8;
        private int right = 328;

        private int wall1 = 88;
        private int wall2 = 248;
        private int wall3 = 128;
        private int wall4 = 288;


        private Timer timer;

        private List<IClient> clients;

        int yellow = 1; //1 if it is moving to the rigth, -1 if it is moving left
        int red = 1; //1 if it is moving to the rigth, -1 if it is moving left
        int pinkx = 1; //1 if it is moving to the rigth, -1 if it is moving left
        int pinky = 1; //1 if it is moving up, -1 if it is moving down

        public GameEngine(List<IClient> clients)
        {
            this.clients = clients;
            int cnt = 0;
            //add all the coins to the dictionary
            for (int i = 8; i <= width; i += 40)
            {
                for (int j = 40; j <= heigth; j += 40)
                {
                    //first top wall
                    if (i == wall1 && (j == 40 || j == 80 || j == 120))
                    {
                        continue;
                    }
                    //second top wall
                    if (i == wall2 && (j == 40 || j == 80 || j == 120))
                    {
                        continue;
                    }
                    //first bottom wall
                    if (i == wall3 && (j == 240 || j == 280 || j == 320))
                    {
                        continue;
                    }
                    //second bottom wall
                    if (i == wall4 && (j == 240 || j == 280 || j == 320))
                    {
                        continue;
                    }
                   
                    cnt++;
                    coins.Add(cnt, new int[] { i, j });

                }
            }


            //add all the walls to the dictionary
            //the walls have all the same size, were y+=40
            walls.Add(1, new int[] { 88, 40 });
            walls.Add(2, new int[] { 248, 40 });
            walls.Add(3, new int[] { 128, 240 });
            walls.Add(4, new int[] { 288, 240 });

            //add all the ghosts to the dictionary
            //pink, the one that moves on diagonals
            ghosts.Add(1, new int[] { 301, 72 });
            //yellow  
            ghosts.Add(2, new int[] { 221, 273 });
            //red 
            ghosts.Add(3, new int[] { 180, 73 });

        }

        public void startTimer(string gamerate)
        {
            timer = new Timer(Int32.Parse(gamerate));
            timer.Elapsed += update;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public Dictionary<string, int[]> getPacmans()
        {
            return pacmans;
        }

        public void setPacmans(string ip, string port)
        {
            pacmans.Add(ip + ":" + port, new int[] { 8, calculatePacmanPosY(), 0 });
        }

        public Dictionary<int, int[]> getCoins()
        {
            return coins;
        }

        public Dictionary<int, int[]> getGhosts()
        {
            return ghosts;
        }

        public Dictionary<string, string> getMoves()
        {
            return moves;
        }

        public void setMoves(string ip, string port, string move)
        {
            moves.Add(ip + ":" + port, move);
        }

        //public Dictionary<string, int> getScore()
        //{
        //    return score;
        //}

        //public void setScore(string ip, string port)
        //{
        //    score.Add(ip + ":" + port, 0);
        //}

        public void seePacmans()
        {
            foreach (KeyValuePair<string, int[]> kvp in pacmans)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine("Key = {0}, Value 0 = {1}, Value 1 = {2}", kvp.Key, kvp.Value[0], kvp.Value[1]);
            }
        }

        public void seeGhosts()
        {
            foreach (KeyValuePair<int, int[]> kvp in ghosts)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine("Key = {0}, Value 0 = {1}, Value 1 = {2}", kvp.Key, kvp.Value[0], kvp.Value[1]);
            }
        }

        public void seeScore()
        {
            foreach(KeyValuePair<string, int[]> kvp in pacmans)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine("Key = {0}, Value 0 = {1}", kvp.Key, kvp.Value[2]);
            }
        }

        public List<IClient> getClients()
        {
            return clients;
        }

        public void seeCoins()
        {
            foreach (KeyValuePair<int, int[]> kvp in coins)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine("Key = {0}, Value 0 = {1}, Value 1 = {2}", kvp.Key, kvp.Value[0], kvp.Value[1]);
            }
        }

        public int calculatePacmanPosY()
        {
            number++;
            return (number*40);
        }

        public void update(object sender, ElapsedEventArgs elapsedEventArg)
        {
            moveRedGhost();
            movePinkGhost();
            moveYellowGhost();
            foreach (KeyValuePair<string, string> entry in moves.ToList())
            {
                //Console.WriteLine("AAAA " + entry + " - " + entry.Key);
                if (pacmans[entry.Key] != null)
                {
                    //Console.WriteLine("vou enviar {0}, {1}, {2}, {3}", pacmans[entry.Key][0], pacmans[entry.Key][1], entry.Value, entry.Key);
                    movePacman(pacmans[entry.Key][0], pacmans[entry.Key][1], entry.Value, entry.Key);
                    //Console.WriteLine("recebi {0}, {1}, {2}, {3}", pacmans[entry.Key][0], pacmans[entry.Key][1], entry.Value, entry.Key);
                    if(hitWall(pacmans[entry.Key][0], pacmans[entry.Key][1], pacmansize))
                    {
                        Console.WriteLine("GAME OVER");
                        pacmans.Remove(entry.Key);  //if lose the key is removed
                    }
                    if(hitGhost(pacmans[entry.Key][0], pacmans[entry.Key][1]))
                    {
                        Console.WriteLine("HIT A GHOST");
                        pacmans.Remove(entry.Key); //if lose the key is removed
                    }
                    if (hitCoin(pacmans[entry.Key][0], pacmans[entry.Key][1]))
                    {
                        //seeScore();
                        pacmans[entry.Key][2] += 1;
                        //seeScore();
                        if(coins.Count == 0)
                        {
                            Console.WriteLine("VICTORY");
                        }
                    }
                }
            }

            if (clients.Count > 0)
            {
                foreach (IClient client in clients)
                {
                    //Console.WriteLine(client.getIP()+":"+client.getPort());
                    client.updateGameState(pacmans, ghosts, coins);
                }
            }

            moves.Clear();

        }

        public void movePacman(int x, int y,  string move, string destination)
        {
            if (move.Equals("left") && x > left)
            {
                x -= 5;  //5 it is pacmans speed
                pacmans[destination][0] = x;
            }

            if (move.Equals("right") && x < right)
            {
                x += 5;
                pacmans[destination][0] = x;
            }
            if (move.Equals("up") && y > top)
            {
                y -= 5;
                pacmans[destination][1] = y;
            }
            if (move.Equals("down") && y < bottom)
            {
                y += 5;
                pacmans[destination][1] = y;
            }
        }

        public void movePinkGhost()
        {
            if(hitWall(ghosts[1][0], ghosts[1][1], ghostsize) || ghosts[1][0] <= (128 + wallWidth) || (ghosts[1][0] + ghostsize) >= (right + coinsize))
            {
                pinkx = -pinkx;
            }
            //if (ghosts[1][0] <= (128 + wallWidth) || ghosts[1][0] >= 328)
            //{
            //    //hit the wall and has to go back
            //    pinkx = -pinkx;
            //}
            if (pinkx == 1)
            {
                ghosts[1][0] += 5;  //5 it is the ghosts speed
            }
            else
            {
                ghosts[1][0] -= 5;
            }
            if (ghosts[1][1] <= top || (ghosts[1][1] + ghostsize) >= bottom)
            {
                //hit the wall and has to go back
                pinky = -pinky;
            }
            if (pinky == 1)
            {
                ghosts[1][1] -= 5;
            }
            else
            {
                ghosts[1][1] += 5;
            }
            //Console.WriteLine("PINK " + ghosts[1][0] + " - " + ghosts[1][1]);

        }

        public void moveRedGhost()
        {
            if (ghosts[3][0] <= (wall1 + wallWidth) || (ghosts[3][0] + ghostsize)>= wall2)
            {
                //hit the wall and has to go back
                red = -red;
            }
            if (red == 1)
            {
                ghosts[3][0] += 5;
            }
            else
            {
                ghosts[3][0] -= 5;
            }
        }

        public void moveYellowGhost()
        {
            if (ghosts[2][0] <= (wall3 + wallWidth) || (ghosts[2][0] + ghostsize) >= wall4)
            {
                //hit the wall and has to go back
                yellow = -yellow;
            }
            if (yellow == 1)
            {
                ghosts[2][0] += 5;
            }
            else
            {
                ghosts[2][0] -= 5;
            }
        }

        public Boolean hitWall(int x, int y, int size)
        {
            //Console.WriteLine("a ver se bateu {0} , {1}", x, y);
            if (y >= top && y < 135)  //135 is the y position where pacman do not hit
            {
                if ((x > (wall1-size) && x < (wall1 + wallWidth)) || (x > (wall2 - size) && x < (wall2 + wallWidth)))
                {
                    //Console.WriteLine("parede de cima");
                    return true;
                }
            }
            if (y > 215 && y <= bottom)  //215 is the y position where pacman do not hit
            {
                if ((x > (wall3 - size) && x < (wall3 + wallWidth)) || (x > (wall4 - size) && x < (wall4 + wallWidth)))
                {
                    //Console.WriteLine("parede de baixo");
                    return true;
                }
            }
            return false;
        }

        public Boolean hitGhost(int x, int y)
        {
           // Console.WriteLine("a ver se bateu em fantasmas {0} , {1}", x, y);
            foreach(KeyValuePair<int, int[]> ghost in ghosts)
            {
                //Console.WriteLine("Ghost {0}, {1}, {2}", ghost.Key, ghosts[ghost.Key][0], ghosts[ghost.Key][1]);
                if ((ghosts[ghost.Key][0] + ghostsize) >= x && ghosts[ghost.Key][0] < (x + pacmansize))
                {
                    if ((ghosts[ghost.Key][1] + ghostsize) >= y && ghosts[ghost.Key][1] < (y + pacmansize))
                    {
                        //one ghost is in the same position as the pacman
                        return true;
                    }    
                }

            }
            return false;
        }

        public Boolean hitCoin(int x, int y)
        {
            foreach(KeyValuePair<int, int[]> coin in coins)
            {
                if(coins[coin.Key][0] == x && coins[coin.Key][1] == y)
                {
                    //Console.WriteLine("MOEDAS " + coins[coin.Key]);
                    coins.Remove(coin.Key);
                    //Console.WriteLine("MOEDAS depois " );
                    //seeCoins();
                    return true;
                }
            }
            return false;
        }
    }

}
