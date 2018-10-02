﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Business;
using Persistence;
using Protocol;
using UI;

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
            var gameLogic = new GameLogic(new Store());

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
                            catch (Exception) //Aca pueden caer SocketExceptions y otras
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

                GoToMenuOption(option, gameLogic);

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
            catch (Exception)
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
                    catch (Exception)
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

        private static void GoToMenuOption(int option, GameLogic controller)
        {
            if (option == 1)
                if (controller.GetClients().Count == 0)
                {
                    Console.WriteLine("There are no logged players.");
                }
            else
                {
                    controller.GetClients().ForEach(client =>
                    {
                        Console.WriteLine(
                            $"- {client.Username} \tConnected: {client.ConnectionsCount} times");
                    });
                }

            else if (option == 2)
            {
                if (controller.GetCurrentPlayers().Count==0)
                {
                    Console.WriteLine("There are no players in current game.");
                }
                else
                {
                    controller.GetCurrentPlayers().ForEach(player =>
                    {
                        if (player.Client.ConnectedSince == null) return;
                        Console.WriteLine(
                            $"- {player.Client.Username} \tConnected: {player.Client.ConnectionsCount}times");
                    });
                } 
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