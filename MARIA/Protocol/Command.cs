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
        ReadAction,
        NotifyNearPlayers,
        NotifyMyOwnDeath,
        TimesOut,
        RemovePlayerFromGame,
        SendPicture
    }
}