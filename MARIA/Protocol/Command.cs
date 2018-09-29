namespace Protocol
{
    public enum Command
    {
        DoAction,
        Login,
        DisconnectClient,
        ListPlayersInGame,
        ListAllClients,
        ListConnectedClients,
        SelectRole,
        JoinGame,
        ReadAction,
        NotifyNearPlayers,
        NotifyMyOwnDeath,
        TimesOut,
        RemovePlayerFromGame,
        EndGame
    }
}