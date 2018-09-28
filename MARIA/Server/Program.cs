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

namespace Server
{
    class Program
    {

        private static bool endServer = false;

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
                    var clientSocket = server.Socket.Accept();
                    var clientThread = new Thread(() => 
                    {
                        //Hay q agregar un while al router.handle de una manera prolija
                   //     while (!endServer)
                     //   {
                            try
                            {
                                router.Handle(new Connection(clientSocket));
                            }catch (Exception e)
                            {
                                endServer = true;
                                //Cuando el cliente pone exit entra aca, esta mal.
                            }
                            
                       // }
                    });
                    clientThread.Start();
                }
            });
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
            thread.Join();
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