using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Business;
using Business.Exceptions;
using Persistence;
using Protocol;
using UI;
using System.Runtime.InteropServices;

namespace Server
{

    class Program
    {

        private static bool endServer = false;
        private static List<Thread> threads = new List<Thread>();
        private static List<Connection> connections = new List<Connection>();
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
            var server = new ServerProtocol();
            int port = GetServerPortFromConfigFile();
            string ip = GetServerIpFromConfigFile();
            server.Start(ip, port);
            var gameLogic = new GameLogic(new Store());

            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
            var thread = new Thread(() =>
            {
                var router = new Router(new ServerController(gameLogic));
                while (!endServer)
                {
                    try
                    {
                        var clientSocket = server.Socket.Accept();
                        var clientThread = new Thread(() =>
                        {
                            try
                            {
                                Connection conn = new Connection(clientSocket);
                                connections.Add(conn);
                                router.Handle(conn);
                            }
                            catch (Exception) 
                            {
                               endServer = true;
                            }
                        });
                        threads.Add(clientThread);
                        clientThread.Start();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" -> The server has stopped listening for connections.");
                    }
                }
            });
            threads.Add(thread);
            thread.Start();
            bool exit = false;
            while (!exit)
            {
                int option = Menus.ServerMainMenu();

                GoToMenuOption(option, gameLogic);

                if (option == 3)
                {
                    endServer = true;
                    exit = true;
                }

            }
            CloseServer(server,gameLogic);
        }

        private static bool GameIsOff(GameLogic controller)
        {
            return controller.Store.ActiveGame == null || !controller.Store.ActiveGame.isOn;
        }

        private static void CloseServer(ServerProtocol server, GameLogic gameLogic)
        {
            try
            {
                server.Socket.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Closing the thread that is listening for connections.");
            }

            if (connections.Count > 0)
            {
                foreach (Connection connection in connections)
                {
                    try
                    {
                        connection.Close();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Forcing socket to close.");
                    }
                }
            }
            CloseThreads();
        }

        private static void CloseThreads()
        {
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            Console.WriteLine("Every thread has been closed. Good-bye.");
        }

        private static void GoToMenuOption(int option, GameLogic controller)
        {
            if (GameIsOff(controller))
            {
                if (option == 1)
                    if (controller.GetClients().Count == 0)
                    {
                        Console.WriteLine("\n -> There are no logged players.\n");
                    }
                    else
                    {
                        Console.WriteLine();
                        controller.GetClients().ForEach(client =>
                        {
                            Console.WriteLine(
                                $"- {client.Username} \tConnected: {client.ConnectionsCount} times");
                        });
                        Console.WriteLine();
                    }

                else if (option == 2)
                {
                    if (controller.GetCurrentPlayers().Count == 0)
                    {
                        Console.WriteLine("\n -> There are no players in current game.\n");
                    }
                    else
                    {
                        Console.WriteLine();
                        controller.GetCurrentPlayers().ForEach(player =>
                        {
                            if (player.Client.ConnectedSince == null) return;
                            Console.WriteLine(
                                $"- {player.Client.Username} \tConnected: {player.Client.ConnectionsCount}times");
                        });
                        Console.WriteLine();
                    }
                }
            }
            else
            {
                Console.WriteLine("\n -> Game in process, try again when it has finished.\n");
            }
        }


        private static string GetServerIpFromConfigFile()
        {
            var appSettings = new AppSettingsReader();
            return (string)appSettings.GetValue("ServerIp", typeof(string));
        }

        private static int GetServerPortFromConfigFile()
        {
            var appSettings = new AppSettingsReader();
            return (int)appSettings.GetValue("ServerPort", typeof(int));
        }

    }

}