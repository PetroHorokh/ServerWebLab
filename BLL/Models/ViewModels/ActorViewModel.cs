namespace BLL.Models.ViewModels;

public class ActorViewModel
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public string? Nationality { get; set; }

    public string? Biography { get; set; }

    public List<MovieSummaryViewModel> Movies { get; set; } = [];

    public string FullName => $"{FirstName} {LastName}";
}
