namespace MafiaPickem.Api.Models.Enums;

public enum MatchState : byte
{
    Upcoming = 0,
    Open = 1,
    Locked = 2,
    Resolved = 3,
    Canceled = 4
}
