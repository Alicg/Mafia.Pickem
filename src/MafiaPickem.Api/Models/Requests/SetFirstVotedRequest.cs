namespace MafiaPickem.Api.Models.Requests;

public class SetFirstVotedRequest
{
    public List<byte> VotedOutSlots { get; set; } = new();
}
