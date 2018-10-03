using System;

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

        public static void InsertUsername()
        {
            Console.WriteLine("Insert Username: ");
        }

        public static void InsertPassword()
        {
            Console.WriteLine("Insert Password: ");
        }

        public static void InsertAvatarPath()
        {
            Console.WriteLine("Insert avatar path: ");
        }

        public static void InvalidCredentials()
        {
            Console.WriteLine("Wrong username or password");
        }

        public static void TheseAreTheConnectedPlayers()
        {
            Console.WriteLine("These are the connected players:");
        }

        public static void LoginSuccessful()
        {
            Console.WriteLine("Logged in successfully");
        }
        
        public static void Clear()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        public static void ClearBoard()
        {
            Console.Clear();
        }

    }

}