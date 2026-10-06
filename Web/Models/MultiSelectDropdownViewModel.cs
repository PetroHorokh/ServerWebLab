using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Models;

public class MultiSelectDropdownViewModel
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Label { get; init; }

    public required string Placeholder { get; init; }

    public required string EmptyText { get; init; }

    public required IReadOnlyList<SelectListItem> Options { get; init; }
}
