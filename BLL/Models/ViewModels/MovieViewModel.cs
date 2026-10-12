namespace BLL.Models.ViewModels;

public class MovieViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public int DurationMinutes { get; set; }

    public string? Director { get; set; }

    public decimal? Rating { get; set; }

    public string? Description { get; set; }

    public List<ActorSummaryViewModel> Actors { get; set; } = [];

    public string DisplayName => $"{Title} ({ReleaseYear})";
}
