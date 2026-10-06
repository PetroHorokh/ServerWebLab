namespace BLL.Models.ViewModels;

public class MovieSummaryViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int ReleaseYear { get; set; }

    public string DisplayName => $"{Title} ({ReleaseYear})";
}
