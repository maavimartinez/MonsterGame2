using System;
using System.Collections.Generic;

namespace UI
{

    public static class Menus
    {
        
        public static int ClientControllerLoopMenu()
        {
            List<string> options = new List<string>(new[]{
                    "List Connected Users",
                    "Upload avatar",
                    "Join Game",
                    "Exit"
            });
            for (var i = 0; i < options.Count; i++)
            {
                Console.WriteLine(i + 1 + " - " + options[i]);
            }
            return Input.SelectMenuOption("Choose an option", 1, options.Count);
        }

        public static int SelectRoleMenu()
        {
            List<string> options = new List<string>(new[]{
                    "Monster", "Survivor", "Exit"
            });
            Console.WriteLine("+----------------------------+"); 
            Console.WriteLine("|           ROLES            |");
            Console.WriteLine("+----------------------------+");
            for (var i = 0; i < options.Count; i++)
            {
                Console.WriteLine(i + 1 + " - " + options[i]);
            }
            return Input.SelectMenuOption("Choose your role", 1, options.Count);
        }

        public static int ServerMainMenu()
        {
            List<string> options = new List<string>(new[]{
                "Show All Players",
                "Show Connected Players",
                "Exit"
            });

            for (var i = 0; i < options.Count; i++)
            {
                Console.WriteLine(i + 1 + " - " + options[i]);
            }
            return Input.SelectMenuOption("Choose an option", 1, options.Count);
        }

    }

}