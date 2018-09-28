using System;
using System.Collections.Generic;

namespace UI
{
    public class ClientUI
    {

        public static string Title(string username = null)
        {
            var t = "";
            t += "          ,-.-.               |                  ,---.               \n";
            t += "          | | |,---.,---.,---.|--- ,---.,---.    |  _.,---.,-.-.,---.\n";
            t += "          | | ||   ||   |`---.|    |---'|        |   |,---|| | ||---'\n";
            t += "          ` ' '`---'`   '`---'`---'`---'`        `---'`---^` ' '`---'\n";

            return t;
        }

        public static string CallToAction()
        {
            return Resources.PressKeyToContinue;
        }

        public static string Connecting()
        {
            return Resources.ConnectingToServer;
        }

        public static string LoginTitle()
        {
            return "+----------------------------+\n|            LOGIN           |\n+----------------------------+";
        }

        public static string InsertUsername()
        {
            return "Insert Username: ";
        }

        public static string InsertPassword()
        {
            return "Insert Password: ";
        }

        public static string InvalidCredentials()
        {
            return "Wrong username or password";
        }

        public static string TheseAreTheConnectedPlayers()
        {
            return "These are the connected players:";
        }

        public static string LoginSuccessful()
        {
            return "Logged in successfully";
        }

        public static string PromptUsername()
        {
            return "Enter a username";
        }
        
        public static void Clear()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }
}