namespace Web.Models;

public class DeleteConfirmationViewModel
{
    public required string Title { get; init; }

    public required string ItemName { get; init; }

    public string? Details { get; init; }

    public required string ActionUrl { get; init; }
}
