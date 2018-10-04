namespace Protocol
{
    public enum Command
    {
        Login,
        DisconnectClient,
        ListPlayersInGame,
        ListAllClients,
        ListConnectedClients,
        SelectRole,
        JoinGame,
        DoAction,
        TimesOut,
        RemovePlayerFromGame,
        ReadyToSendPicture,
        SendPicturePart,
        SendLastPicturePart,
        CheckIfGameHasFinished,
        GetResultByTimesOut
    }

}