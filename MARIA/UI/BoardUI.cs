using System;
using System.Collections.Generic;
using Entities;

namespace UI
{
    public static class BoardUI
    {

        public static void WriteRules()
        {
            Console.WriteLine("Move:   MOVLetterNumber (e.g MOVA5)");
            Console.WriteLine("Attack: ATTUsername     (e.g ATTexample)");
            Console.WriteLine("Commands ignore case and blank spaces");
            Console.WriteLine("");
        }

        public static void WriteOnGameUsernames(List<string> opponents)
        {
            Console.WriteLine("On-Game Players: ");
            foreach(string st in opponents)
            {
                Console.WriteLine("->" + st);
            }
        }

        public static void DrawBoard(string username, string playerPosition){
            Console.Clear();
            Console.WriteLine(ClientUI.Title());
            WriteRules();
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