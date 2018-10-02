using System;
using System.Collections.Generic;

namespace UI
{

    public static class BoardUI
    {

        public static void DrawBoard(string username, string playerPosition, List<string> usernamesAndStatus){
            Console.Clear();
            Console.WriteLine(ClientUI.Title());
            ShowShortRules();
            ShowOnGameUsernamesAndStatus(usernamesAndStatus);
            int[] pos = GetIntPosition(playerPosition);
            if (pos != null)
            {
                string initial = GetInitial(username);
                int a = 64;
                char myChar;
                Console.WriteLine("   1   2   3   4   5   6   7   8 ");
                Console.WriteLine(" +---+---+---+---+---+---+---+---+");
                for (int i = 0; i < 8; i++)
                {
                    a++;
                    myChar = (char)a;
                    Console.Write(myChar + "|");
                    for (int j = 0; j < 8; j++)
                    {
                        if (i == pos[0] && j == pos[1])
                        {
                            Console.Write(" " + initial + " |");
                        }
                        else
                        {
                            Console.Write("   " + "|");
                        }
                    }
                    Console.WriteLine("");
                    if (i < 8 - 1)
                    {
                        Console.WriteLine(" +---+---+---+---+---+---+---+---+");
                    }
                }
                Console.WriteLine(" +---+---+---+---+---+---+---+---+");
            }
        }

        private static void ShowShortRules()
        {
            Console.WriteLine("Move:   MOVLetterNumber (e.g MOVA5)");
            Console.WriteLine("Attack: ATTUsername     (e.g ATTexample)");
            Console.WriteLine("- Moves can be made in all directions with radius 1.");
            Console.WriteLine("- Commands ignore case and blank spaces");
            Console.WriteLine("");
        }

        private static void ShowOnGameUsernamesAndStatus(List<string> opponents)
        {
            if (opponents.Count > 0)
            {
                Console.WriteLine("On-Game Players: ");
                foreach (string st in opponents)
                {
                    Console.WriteLine("->" + st);
                }
            }
        }

        public static void ShowHP(string hp)
        {
            if (hp != "")
            {
                Console.WriteLine("HP = " + hp);
            }
        }

        public static void ShowKills(string kill)
        {
            if(kill != "")
            {
                Console.WriteLine("You have killed " + kill + " !");
            }       
        }

        public static void ShowNearPlayers(List<string> nearPlayers)
        {
            if (nearPlayers.Count != 0)
            {
                Console.WriteLine("You are next to: ");
                foreach (string st in nearPlayers)
                {
                    Console.WriteLine(st);
                }
            }
        }

        private static string GetInitial(string username)
        {
            return (username[0] + "").ToUpper();
        }

        private static int[] GetIntPosition(string position)
        {
            try
            {
                int[] pos = new int[2];
                string[] aux = position.Split('!');
                pos[0] = Int32.Parse(aux[1]);
                pos[1] = Int32.Parse(aux[0]);
                return pos;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

}