using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameEngine
    {
        //saves all coins positions
        public static Dictionary<int, string[]> coins = new Dictionary<int, string[]>();
        //saves all walls positions
        public static Dictionary<int, string[]> walls = new Dictionary<int, string[]>();
        //saves all ghosts positions
        public static Dictionary<int, string[]> ghosts = new Dictionary<int, string[]>();
        //saves all pacmans ip:port, number
        public static Dictionary<string, int> pacmans = new Dictionary<string, int>();

        private int width = 328;
        private int heigth = 320;

        public GameEngine()
        {
            int cnt = 0;
            //add all the coins to the dictionary
            for (int i = 8; i <= width; i += 40)
            {
                for (int j = 40; j <= heigth; j += 40)
                {
                    //first top wall
                    if (i == 88 && j == 40 || i == 88 && j == 80 || i == 88 && j == 120)
                    {
                        continue;
                    }
                    //second top wall
                    if (i == 248 && j == 40 || i == 248 && j == 80 || i == 248 && j == 120)
                    {
                        continue;
                    }
                    //first bottom wall
                    if (i == 128 && j == 240 || i == 128 && j == 280 || i == 128 && j == 320)
                    {
                        continue;
                    }
                    //second bottom wall
                    if (i == 288 && j == 240 || i == 288 && j == 280 || i == 288 && j == 320)
                    {
                        continue;
                    }
                   
                    cnt++;
                    coins.Add(cnt, new string[] { i.ToString(), j.ToString() });

                }
            }


            //add all the walls to the dictionary
            //the walls have all the same size, were y+=40
            walls.Add(1, new string[] { "88", "40" });
            walls.Add(2, new string[] { "248", "40" });
            walls.Add(3, new string[] { "128", "240" });
            walls.Add(4, new string[] { "288", "240" });

            //add all the ghosts to the dictionary
            //pink
            ghosts.Add(1, new string[] { "301", "72" });
            //yellow  
            ghosts.Add(2, new string[] { "221", "273" });
            //red 
            ghosts.Add(3, new string[] { "180", "73" });

        }

        public Dictionary<string, int> getPacmans()
        {
            return pacmans;
        }

        public void seePacmans()
        {
            foreach (KeyValuePair<string, int> kvp in pacmans)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine("Key = {0}, Value 0 = {1}", kvp.Key, kvp.Value);
            }
        }

        

    }

}
