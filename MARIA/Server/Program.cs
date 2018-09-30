using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Business;
using Persistence;
using Protocol;
using UI;
using System.Net.Sockets;

namespace Server
{
    class Program
    {

        private static bool endServer = false;
        private static List<Thread> threads = new List<Thread>();
        private static List<Connection> connections = new List<Connection>();

        static void Main(string[] args)
        {
            var server = new ServerProtocol();
            int port = GetServerPortFromConfigFile();
            string ip = GetServerIpFromConfigFile();
            server.Start(ip, port);
            var gameController = new GameController(new Store());

            var thread = new Thread(() =>
            {
                var router = new Router(new ServerController(gameController));
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
                            catch (Exception e) //Aca pueden caer SocketExceptions y otras
                            {
                                endServer = true;    
                        }
                        });
                        threads.Add(clientThread);
                        clientThread.Start();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("The server has stopped listening for connections.");
                    }
                }
            });
            threads.Add(thread);
            thread.Start();
            bool exit = false;
            while (!exit)
            {
                int option = Menus.ServerMainMenu();

                GoToMenuOption(option, gameController);

                if (option == 3)
                {
                    endServer = true; 
                    exit = true;
                }
            }
            CloseServer(server);
        }

        private static void CloseServer(ServerProtocol server)
        {
            //Thread se cuelga en el Accept. Tenemos que matar ese socket.
            try
            {
                server.Socket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cerrando el hilo que escucha conecciones.");
            }

            if (connections.Count > 0)
            {
                foreach (Connection connection in connections)
                {
                    try
                    {
                        //Este mata la conexion al socket del router, pero el catch se hace arriba, no aca. Esta mal?
                        connection.Close();

                    }
                    catch (Exception ex)
                    {
                        //Aca no entra nunca
                        Console.WriteLine("Forzando el socket a cerrar.");
                    }
                }
            }
            else
            {
                //No hay conexiones, no cerramos una a una. Borrar else.
            }
            //Juntando cada thread con el principal..Despues, se cierra la consola.
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


        private static void GoToMenuOption(int option, GameController controller)
        {
            if (option == 1)
                controller.GetClients().ForEach(client =>
                {
                    Console.WriteLine(
                        $"- {client.Username} \tConnected: {client.ConnectionsCount} times");
                });
            else if (option == 2)
            {
                controller.GetLoggedClients().ForEach(client =>
                {
                    if (client.ConnectedSince == null) return;
                    TimeSpan timeConnected = DateTime.Now.Subtract((DateTime)client.ConnectedSince);
                    string timeConnectedFormatted = timeConnected.ToString(@"hh\:mm\:ss");
                    Console.WriteLine(
                        $"- {client.Username} \tConnected: {client.ConnectionsCount} times \tConnected for: {timeConnectedFormatted}");
                });
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