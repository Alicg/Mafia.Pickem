namespace MafiaPickem.Api.Models.Responses;

public class SlotVoteDto
{
    public byte Slot { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
