using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Linq;
using Protocol;
using UI;
using System.Text;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;


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
        private Connection TimeControllerConnection { get; set; }
        private bool timesOut = false;
        private bool exitGame = false;
        private bool playerIsDead = false;
        private Thread timer;

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
                PrepareSendingImage(client.Username);
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
            string username = Input.RequestUsernameAndPassword(ClientUI.InsertUsername());

            Console.WriteLine(ClientUI.InsertPassword());
            string password = Input.RequestUsernameAndPassword(ClientUI.InsertUsername());

            return new Entities.Client(username, password);
        }

        private object[] BuildRequest(Command command, params object[] payload)
        {
            List<object> request = new List<object>(payload);
            request.Insert(0, command.GetHashCode());
            request.Insert(1, clientToken);

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
                    break;
            }
        }

        private void Play()
        {
            playerIsDead = false;
            exitGame = false;
            timesOut = false;
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

            List<string> onGameUsernamesAndStatus = response.GetOnGameUsernamesAndStatus();
            BoardUI.DrawBoard(clientUsername, response.GetPlayerPosition(), onGameUsernamesAndStatus);
            Console.WriteLine("Action: ");

            if (response.HadSuccess())
            {
                if (timer == null)
                {
                   timer = new Thread(() => TimesOut());
                   timer.Start();
                }

                while ((!exitGame || !timesOut) && !playerIsDead)
                {

                    string myAction = Input.RequestInput();
                     
                    if (timesOut) break;

                    if (myAction.Equals("exit"))
                    {
                        RemovePlayerFromGame();
                        exitGame = true;
                    }
                    else
                    {
                        SocketConnection.SendMessage(BuildRequest(Command.DoAction, myAction));

                        var sendActionResponse = new Response(SocketConnection.ReadMessage());

                        if (sendActionResponse.HadSuccess())
                        {
                            List<string> actionResponse = sendActionResponse.GetDoActionResponse();
                            RefreshBoard(actionResponse);
                            ShowIfGameFinished(actionResponse,false);
                        }
                        else if (sendActionResponse.IsInvalidAction())
                        {
                            Console.WriteLine(sendActionResponse.ErrorMessage());
                            if (sendActionResponse.ErrorMessage() == "You are dead and can no longer play")
                            {
                                playerIsDead = true;
                            }
                            else
                            {
                                Console.WriteLine("Action: ");
                            }
                        }
                        else
                        {
                            Console.WriteLine(sendActionResponse.ErrorMessage());
                            RemovePlayerFromGame();
                        }
                    }
                }
                if (playerIsDead)
                {
                    string st = AskServerIfGameHasFinished();
                }
            }
            else
            {
                Console.WriteLine(response.ErrorMessage());
                string st = AskServerIfGameHasFinished();
            }
            
        }

        public void RemovePlayerFromGame()
        {
            SocketConnection.SendMessage(BuildRequest(Command.RemovePlayerFromGame));

            var response = new Response(SocketConnection.ReadMessage());

            if (response.HadSuccess())
            {
                ShowIfGameFinished(response.GetRemovePlayerFromGameResponse(),false);
            }
            else 
            {
                Console.WriteLine(response.ErrorMessage());
            }
        }

        private string AskServerIfGameHasFinished()
        {
            bool exit = false;
            while (!exit) {
                SocketConnection.SendMessage(BuildRequest(Command.CheckIfGameHasFinished));

                var response = new Response(SocketConnection.ReadMessage());

                if (response.HadSuccess())
                {
                    string result = response.GetGameResult();
                    if(result != "GameNotFinished")
                    {
                        Console.WriteLine(result);
                        exit = true;
                        return "GameFinished";
                    }else
                    {
                        return "GameNotFinished";
                    }
                }
                else
                {
                    Console.WriteLine(response.ErrorMessage());
                    exit = true;
                }
            }
            return null; //chequear
        }

        private void TimesOut()
        {
            TimeControllerConnection = clientProtocol.ConnectToServer();
            while (!timesOut)
            {
                    TimeControllerConnection.SendMessage(BuildRequest(Command.TimesOut));

                    var sendActionResponse = new Response(TimeControllerConnection.ReadMessage());

                if (sendActionResponse.GameHasFinished())
                {
                    GetResultByTimesOut();
                }
                   
            }
        }

        private void GetResultByTimesOut()
        {
            SocketConnection.SendMessage(BuildRequest(Command.GetResultByTimesOut));

            var response = new Response(SocketConnection.ReadMessage());

            ShowIfGameFinished(response.GetTimeOutResponse(), true);
        }

        private void  ShowIfGameFinished(List<string> responseMessage, bool timesOut2)
        {
            for(int i = 0; i< responseMessage.Count(); i++)
            {
                if(responseMessage[i] == "FINISHED")
                {
                    if (timesOut2)  Console.WriteLine("Active Game's time is over!. You can now join a new game.");
                  //  if (!timesOut2) Console.WriteLine("Game is over! ");
                    Console.WriteLine(responseMessage[i + 1]);
                    exitGame = true;
                    timesOut = true;
                    timer = null;
                }
            }
        }

        private void RefreshBoard(List<string> response)
        {
            BoardUI.DrawBoard(clientUsername, response[0], GetUsernamesAndStatus(response));
            BoardUI.ShowHP(GetHP(response));
            BoardUI.ShowKills(GetKills(response));
            BoardUI.ShowNearPlayers(GetNearPlayers(response));
            Console.WriteLine("Action: ");
        }

        private List<string> GetUsernamesAndStatus(List<string> response)
        {
            List<string> usernamesStatus = new List<string>();
            for (int i = 0; i < response.Count(); i++)
            {
                if (response[i].Equals("PLAYERS"))
                {
                    for (int j = i + 1; j < response.Count(); j++)
                    {
                        usernamesStatus.Add(response[j]);
                    }
                }
            }
            return usernamesStatus;
        }

        private string GetHP(List<string> response)
        {
            for(int i=0; i<response.Count(); i++)
            {
                if (response[i].Equals("HP"))
                    return response[i + 1];
            }
            return "";
        }

        private string GetKills(List<string> response)
        {
            for (int i = 0; i < response.Count(); i++)
            {
                if (response[i].Equals("KILLED"))
                {
                    return response[i + 1];
                }
            }
            return "";
        }

        private List<string> GetNearPlayers(List<string> response)
        {
            List<string> near = new List<string>();
            for (int i = 0; i < response.Count(); i++)
            {
                if (response[i].Equals("NEAR"))
                {
                    for (int j = i + 1; j < response.Count() && !response[j].Equals("HP") && !response[j].Equals("FINISHED"); j++)
                    {
                        near.Add(response[j]);
                    }
                }
            }
            return near;
        }

         private void PrepareSendingImage(string username)
         {
             //Esta foto pesa 54 kb, va de ejemplo
             string path = @"C:\Users\Usuario\Desktop\03.png";

             FileInfo fileInfo = new FileInfo(path);

             //Osea 54593 bytes
             byte[] data = new byte[fileInfo.Length];

             int totalLength = data.Length;

            byte[] parts = new byte[9999];

            Command command = Command.SendPicturePart;

            int times = totalLength / 9999;

            SocketConnection.SendMessage(BuildRequest(Command.ReadyToSendPicture, username, totalLength, path));
            var response = new Response(SocketConnection.ReadMessage());

            if (response.HadSuccess())
            {
                // Load a filestream and put its content into the byte[]
                using (FileStream fs = fileInfo.OpenRead())
                {
                    var read = 0;
                    while (read < totalLength)
                    {
                        if (times < 1)
                        {
                            command = Command.SendLastPicturePart;
                        }
                        read += fs.Read(data, read, 9999);
                        Array.Copy(data, 0, parts, 0, 9999);

                        //Socket send mis bytes hasta ahora

                        //NO SE COMO PASAR EL ARRAY DE BYTES parts en la request

                        string converted = Encoding.UTF8.GetString(parts, 0, parts.Length);

                        //prueba la reconversion q se va a hacer en serverController
                        byte[] receivedParts = Encoding.ASCII.GetBytes(converted);

                        SocketConnection.SendMessage(BuildRequest(command, converted));
                        var keepSendingResponse = new Response(SocketConnection.ReadMessage());

                        //Clear el array q mando
                        Array.Clear(parts, 0, 9999);
                        times--;
                    }
                }
            }else
            {
                Console.WriteLine(response.ErrorMessage());
            }
         }
    }
}
