using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;


namespace Client
{
    class Program
    {
        private static ClientController clientController = new ClientController();

        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        static void Main(string[] args)
        {
            try
            {
              //  DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
                clientController.LoopMenu();
            }
            catch (SocketException e)
            {
                Console.WriteLine("There was a problem connecting to the server, the app will exit");
                Console.ReadKey();
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was a problem with something you did, the app will exit");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                Environment.Exit(1);
            }
        }

    }
}