namespace MafiaPickem.Api.Models.Requests;

public class ResolveMatchRequest
{
    public byte WinningSide { get; set; }
    public List<byte> VotedOutSlots { get; set; } = new();
}
