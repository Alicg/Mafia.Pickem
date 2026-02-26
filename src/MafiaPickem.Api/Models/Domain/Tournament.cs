namespace MafiaPickem.Api.Models.Domain;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool Active { get; set; }
    public DateTime DateCreated { get; set; }
}
