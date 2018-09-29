﻿using System;
using System.Collections.Generic;
using Protocol;
using UI;
using System.Configuration;
using Business;
using System.Threading;
using System.Linq;
using Entities;


namespace Client
{
    public class ClientController
    {
        private const double WaitTimeAumentation = 1.5;
        private const int InitialWaitTime = 100;
        private readonly ClientProtocol clientProtocol;
        private string clientToken;
        private string clientUsername;
        private Connection SocketConnection { get; set; }
        private bool timesOut = false;
        private bool exitGame = false;

        public ClientController()
        {
            clientToken = "";
            clientUsername = null;
            string serverIp = GetServerIp();
            int serverPort = GetServerPort();
            string clientIp = GetClientIp();
            int clientPort = GetClientPort();
            clientProtocol = new ClientProtocol(serverIp, serverPort, clientIp, clientPort);
        }

        private void ConnectToServer()
        {
            Console.WriteLine(ClientUI.Connecting());
            bool connected;
            do
            {
                Entities.Client client = AskForCredentials();
                SocketConnection = clientProtocol.ConnectToServer();
                object[] request = BuildRequest(Command.Login, client.Username, client.Password);
                SocketConnection.SendMessage(request);
                var response = new Response(SocketConnection.ReadMessage());
                connected = response.HadSuccess();
                if (connected)
                {
                    clientToken = response.GetClientToken();
                    clientUsername = client.Username;
                    Console.WriteLine(ClientUI.LoginSuccessful());
                }
                else
                {
                    Console.WriteLine(response.ErrorMessage());
                }
            } while (!connected);
        }

        public void DisconnectFromServer()
        {
            SocketConnection.SendMessage(BuildRequest(Command.DisconnectClient));
            var response = new Response(SocketConnection.ReadMessage());
            Console.WriteLine(response.HadSuccess() ? "Disconnected" : response.ErrorMessage());
        }

        internal void LoopMenu()
        {
            Init();
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine(ClientUI.Title(clientUsername));
                int option = Menus.ClientControllerLoopMenu();
                if (option == 3) exit = true;
                MapOptionToActionOfMainMenu(option);
                ClientUI.Clear();
            }
        }

        private void Init()
        {
            Console.WriteLine(ClientUI.Title());
            ConnectToServer();
            ClientUI.Clear();
        }

        private bool ListConnectedClients()
        {
            bool serverHasClients;
            object[] request = BuildRequest(Command.ListConnectedClients);
            SocketConnection.SendMessage(request);

            var response = new Response(SocketConnection.ReadMessage());
            if (response.HadSuccess())
            {
                Console.WriteLine(ClientUI.TheseAreTheConnectedPlayers());
                List<string> connectedClients = response.UserList();
                PrintPlayers(connectedClients);
                serverHasClients = connectedClients.Count > 0;
            }
            else
            {
                Console.WriteLine(response.ErrorMessage());
                serverHasClients = false;
            }
            return serverHasClients;
        }

        private List<string> GetListOfAllClients()
        {
            var clients = new List<string>();
            object[] request = BuildRequest(Command.ListAllClients);
            SocketConnection.SendMessage(request);

            var response = new Response(SocketConnection.ReadMessage());
            if (response.HadSuccess())
            {
                clients = response.UserList();
            }

            return clients;
        }

        private List<string> ListPlayersInGame()
        {
            var friends = new List<string>();

            object[] request = BuildRequest(Command.ListPlayersInGame);
            SocketConnection.SendMessage(request);

            var response = new Response(SocketConnection.ReadMessage());
            if (response.HadSuccess())
            {
                friends = response.UserList();
            }
            else
            {
                Console.WriteLine(response.ErrorMessage());
            }

            return friends;
        }

        private void PrintPlayers(List<string> players)
        {
            players.ForEach(Console.WriteLine);
        }

        
        private string GetServerIp()
        {
            var appSettings = new AppSettingsReader();
            return (string)appSettings.GetValue("ServerIp", typeof(string));
        }

        private int GetServerPort()
        {
            var appSettings = new AppSettingsReader();
            return (int)appSettings.GetValue("ServerPort", typeof(int));
        }

        private string GetClientIp()
        {
            var appSettings = new AppSettingsReader();
            return (string)appSettings.GetValue("ClientIp", typeof(string));
        }

        private int GetClientPort()
        {
            var appSettings = new AppSettingsReader();
            return (int)appSettings.GetValue("ClientPort", typeof(int));
        }

        private Entities.Client AskForCredentials()
        {
            Console.WriteLine(ClientUI.LoginTitle());

            Console.WriteLine(ClientUI.InsertUsername());
            string username = Input.RequestInput();

            Console.WriteLine(ClientUI.InsertPassword());
            string password = Input.RequestInput();

            return new Entities.Client(username, password);
        }

        private object[] BuildRequest(Command command, params object[] payload)
        {
            List<object> request = new List<object>(payload);
            request.Insert(0, new object[] { command.GetHashCode(), clientToken });

            return request.ToArray();
        }

        private void MapOptionToActionOfMainMenu(int option)
        {
            switch (option)
            {
                case 1:
                    ListConnectedClients();
                    break;
                case 2:
                    Play();
                    break;
                default:
                    DisconnectFromServer();
                  //  Environment.Exit(0);
                    break;
            }
        }

        private void Play()
        {
            int input = Menus.SelectRoleMenu();
            input --;
            string role="";
            if(input == 0) role = "Monster";
            if(input == 1) role = "Survivor";

            SocketConnection.SendMessage(BuildRequest(Command.SelectRole, role));

            var response = new Response(SocketConnection.ReadMessage());

            if (response.HadSuccess())
            {
                Console.WriteLine("You are now a " + role);
                JoinGame();
            }
            else
            {
                Console.WriteLine(response.ErrorMessage());
            }
        }

        private void JoinGame()
        {
            SocketConnection.SendMessage(BuildRequest(Command.JoinGame));

            var response = new Response(SocketConnection.ReadMessage());
            

            BoardUI.DrawBoard(clientUsername, response.GetPlayerPosition());
            Console.WriteLine("Action: ");

            if (response.HadSuccess())
            {
                //Anda, ver si hacerlo en otro connection (socket) o aca esta bn
                var timeThread = new Thread(() => TimesOut());
                timeThread.Start();

                while (!exitGame && !timesOut)
                {

                    string myAction = Input.RequestInput();

                    if (myAction.Equals("exit"))
                    {
                        exitGame = true;
                    }
                    else
                    {
                        SocketConnection.SendMessage(BuildRequest(Command.DoAction, myAction));

                        var sendActionResponse = new Response(SocketConnection.ReadMessage());

                        if (sendActionResponse.HadSuccess())
                        { 
                            List<string> positionKillsAndNearPlayers = sendActionResponse.GetDoActionResponse();
                            RefreshBoard(positionKillsAndNearPlayers);
                        }
                        else if(sendActionResponse.IsInvalidAction())
                        {
                            Console.WriteLine(sendActionResponse.ErrorMessage());
                            Console.WriteLine("Action: ");
                        }
                        else if (sendActionResponse.PlayerHasWon())
                        {
                            Console.WriteLine(sendActionResponse.ErrorMessage()); 
                            EndGame();
                        }
                        else 
                        {
                            Console.WriteLine(sendActionResponse.ErrorMessage());
                            exitGame = true;
                            RemovePlayerFromGame();
                        }
                    }

                }
                //Cambiar
                timeThread.Abort();
            }
            else
            {
                Console.WriteLine(response.ErrorMessage());
            }
            
        }

        private void RemovePlayerFromGame()
        {
            SocketConnection.SendMessage(BuildRequest(Command.RemovePlayerFromGame));

            var response = new Response(SocketConnection.ReadMessage());

            if (!response.HadSuccess())
            {
                Console.WriteLine(response.ErrorMessage());
            }
        }

        private void EndGame()
        {
            SocketConnection.SendMessage(BuildRequest(Command.EndGame));

            var response = new Response(SocketConnection.ReadMessage());

            if (!response.HadSuccess())
            {
                Console.WriteLine(response.ErrorMessage());
            }
        }

        private void TimesOut()
        {
            while (!timesOut)
            {
                SocketConnection.SendMessage(BuildRequest(Command.TimesOut));

                var sendActionResponse = new Response(SocketConnection.ReadMessage());

                if (sendActionResponse.HadSuccess())
                {
                    if (sendActionResponse.GetRemainingTime().Equals("timesOut"))
                    {
                        Console.WriteLine("Time's over !");
                        exitGame = true; //Aca hay que inhabilitar el doAction-- exit game y timesout deberian ser el mismo bool
                        timesOut = true;
                        EndGame();
                    }
                }
            }
        }

        private void RefreshBoard(List<string> position)
        {
            BoardUI.DrawBoard(clientUsername, position[0]);
            ShowKillsAndNearPlayers(position);
            Console.WriteLine("Action: ");
        }

        private void ShowKillsAndNearPlayers(List<string> killsAndNear)
        {
            if (killsAndNear.Count > 0)
            {
                for (int i = 0; i < killsAndNear.Count; i++)
                {
                    if (killsAndNear[i] == "killed")
                    {
                        Console.WriteLine("You have killed " + killsAndNear[i + 1] + " !");
                    }
                    else if (killsAndNear[i] == "near")
                    {
                        Console.WriteLine("You are next to: ");
                        for(int j = i+1; j< killsAndNear.Count; j++)
                        {
                            Console.WriteLine(killsAndNear[j]);
                        }
                        i = killsAndNear.Count;
                    }
                }
            }
        }

    }
}