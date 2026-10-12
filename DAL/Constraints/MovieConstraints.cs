namespace DAL.Constraints;

public static class MovieConstraints
{
    public const int TitleMaxLength = 200;

    public const int GenreMaxLength = 50;

    public const int DirectorMaxLength = 150;

    public const int DescriptionMaxLength = 2000;

    public const int MinReleaseYear = 1888;

    public const int MaxReleaseYear = 2100;

    public const int MinDurationMinutes = 1;

    public const int MaxDurationMinutes = 600;

    public const double MinRating = 0;

    public const double MaxRating = 10;

    public const int RatingPrecision = 3;

    public const int RatingScale = 1;
}
