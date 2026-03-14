namespace MafiaPickem.Api.Models.Enums;

/// <summary>
/// Прогноз на последний круг (зависит от победившей стороны).
/// </summary>
public enum LastRound : byte
{
    None = 0,
    TownClean = 1,   // Победа города — сухая
    TownGuess = 2,   // Победа города — другое
    Mafia3v3 = 3,    // Победа мафии — 3в3
    Mafia2v2 = 4,    // Победа мафии — 2в2
    Mafia1v1 = 5     // Победа мафии — 1в1
}
