USE [Cinema];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @AdminId uniqueidentifier = '507a1793-3e95-4d1c-8a91-e16ba148a876';
DECLARE @Now datetime2 = SYSUTCDATETIME();

DECLARE @Inception uniqueidentifier = '05ed5b37-4be2-40df-864a-9dbdec2ce128';
DECLARE @DarkKnight uniqueidentifier = '97d75ad0-e267-430b-a12d-ef9887387601';
DECLARE @Interstellar uniqueidentifier = '5b4f8504-3e55-45c8-ba91-eab7dcafdfb0';
DECLARE @WolfOfWallStreet uniqueidentifier = '0ef028c0-8ce1-4075-9c97-d46a4db9a5ea';
DECLARE @Revenant uniqueidentifier = 'a2174cc9-4547-42d5-b9fc-bb7eeca2428e';

DECLARE @DiCaprio uniqueidentifier = '81586e36-1a85-44ee-bab4-3efe80ec9bbf';
DECLARE @Hardy uniqueidentifier = '5ae5fa0b-cfc3-4e26-a368-d88f041608b6';
DECLARE @Caine uniqueidentifier = 'b53a25e0-afdd-441f-9f26-372b4a59a178';
DECLARE @McConaughey uniqueidentifier = '5842a318-5551-46b3-9f5e-36ab00b27fce';
DECLARE @Bale uniqueidentifier = '04b00a37-5f6b-472d-9b2d-764e4b30cfdc';
DECLARE @Ledger uniqueidentifier = 'af30625c-2695-4ea5-845a-7e684e31a4c6';
DECLARE @Hathaway uniqueidentifier = '496d822f-dd1c-41ba-ab6e-86049bdc0e93';
DECLARE @Robbie uniqueidentifier = '5f4a90f4-7fe3-453d-aac7-fc0888cf5ae1';
DECLARE @Johansson uniqueidentifier = '9953fd8c-9526-400e-af77-b0f9d8d5fcd9';
DECLARE @Gosling uniqueidentifier = '39d334e5-8461-47db-8531-05d54e663247';

BEGIN TRANSACTION;

INSERT INTO dbo.Movies (Id, Title, Genre, ReleaseYear, DurationMinutes, Director, Rating, Description, CreatedBy, CreatedDateTime)
SELECT v.Id, v.Title, v.Genre, v.ReleaseYear, v.DurationMinutes, v.Director, v.Rating, v.Description, @AdminId, @Now
FROM (VALUES
    (@Inception, N'Inception', N'Sci-Fi', 2010, 148, N'Christopher Nolan', 8.8,
        N'A thief who steals corporate secrets through dream-sharing technology is given the inverse task of planting an idea into the mind of a CEO.'),
    (@DarkKnight, N'The Dark Knight', N'Action', 2008, 152, N'Christopher Nolan', 9.0,
        N'Batman, Lieutenant Gordon and District Attorney Harvey Dent take on the Joker, a criminal mastermind who plunges Gotham City into anarchy.'),
    (@Interstellar, N'Interstellar', N'Sci-Fi', 2014, 169, N'Christopher Nolan', 8.7,
        N'A team of explorers travels through a wormhole near Saturn in search of a new home for humanity.'),
    (@WolfOfWallStreet, N'The Wolf of Wall Street', N'Crime', 2013, 180, N'Martin Scorsese', 8.2,
        N'The rise and fall of stockbroker Jordan Belfort, whose firm built a fortune on fraud and excess.'),
    (@Revenant, N'The Revenant', N'Adventure', 2015, 156, N'Alejandro G. Iñárritu', 8.0,
        N'A frontiersman left for dead after a bear attack fights his way back through the winter wilderness to find the man who betrayed him.')
) AS v (Id, Title, Genre, ReleaseYear, DurationMinutes, Director, Rating, Description)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Movies m WHERE m.Id = v.Id);

INSERT INTO dbo.Actors (Id, FirstName, LastName, DateOfBirth, Nationality, Biography, CreatedBy, CreatedDateTime)
SELECT v.Id, v.FirstName, v.LastName, v.DateOfBirth, v.Nationality, v.Biography, @AdminId, @Now
FROM (VALUES
    (@DiCaprio, N'Leonardo', N'DiCaprio', CAST('1974-11-11' AS date), N'American',
        N'Won the Academy Award for Best Actor for The Revenant.'),
    (@Hardy, N'Tom', N'Hardy', CAST('1977-09-15' AS date), N'British',
        N'English actor known for playing Bane, Mad Max and the Kray twins.'),
    (@Caine, N'Michael', N'Caine', CAST('1933-03-14' AS date), N'British',
        N'Two-time Academy Award winner whose career spans more than six decades.'),
    (@McConaughey, N'Matthew', N'McConaughey', CAST('1969-11-04' AS date), N'American',
        N'Won the Academy Award for Best Actor for Dallas Buyers Club.'),
    (@Bale, N'Christian', N'Bale', CAST('1974-01-30' AS date), N'British',
        N'Known for transformative roles; won an Academy Award for The Fighter.'),
    (@Ledger, N'Heath', N'Ledger', CAST('1979-04-04' AS date), N'Australian',
        N'Received a posthumous Academy Award for his performance as the Joker.'),
    (@Hathaway, N'Anne', N'Hathaway', CAST('1982-11-12' AS date), N'American',
        N'Won the Academy Award for Best Supporting Actress for Les Misérables.'),
    (@Robbie, N'Margot', N'Robbie', CAST('1990-07-02' AS date), N'Australian',
        N'Actor and producer known for I, Tonya and Barbie.'),
    (@Johansson, N'Scarlett', N'Johansson', CAST('1984-11-22' AS date), N'American',
        N'Known for Lost in Translation, Marriage Story and the Marvel films.'),
    (@Gosling, N'Ryan', N'Gosling', CAST('1980-11-12' AS date), N'Canadian',
        N'Known for Drive, La La Land and Barbie.')
) AS v (Id, FirstName, LastName, DateOfBirth, Nationality, Biography)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Actors a WHERE a.Id = v.Id);

INSERT INTO dbo.MovieActors (MovieId, ActorId, CharacterName, IsLeadRole, BillingOrder, CreatedBy, CreatedDateTime)
SELECT v.MovieId, v.ActorId, v.CharacterName, v.IsLeadRole, v.BillingOrder, @AdminId, @Now
FROM (VALUES
    (@Inception, @DiCaprio, N'Dom Cobb', CAST(1 AS bit), 1),
    (@Inception, @Hardy, N'Eames', CAST(0 AS bit), 2),
    (@Inception, @Caine, N'Professor Stephen Miles', CAST(0 AS bit), 3),
    (@DarkKnight, @Bale, N'Bruce Wayne / Batman', CAST(1 AS bit), 1),
    (@DarkKnight, @Ledger, N'The Joker', CAST(1 AS bit), 2),
    (@DarkKnight, @Caine, N'Alfred Pennyworth', CAST(0 AS bit), 3),
    (@Interstellar, @McConaughey, N'Joseph Cooper', CAST(1 AS bit), 1),
    (@Interstellar, @Hathaway, N'Dr. Amelia Brand', CAST(0 AS bit), 2),
    (@Interstellar, @Caine, N'Professor John Brand', CAST(0 AS bit), 3),
    (@WolfOfWallStreet, @DiCaprio, N'Jordan Belfort', CAST(1 AS bit), 1),
    (@WolfOfWallStreet, @Robbie, N'Naomi Lapaglia', CAST(0 AS bit), 2),
    (@WolfOfWallStreet, @McConaughey, N'Mark Hanna', CAST(0 AS bit), 3),
    (@Revenant, @DiCaprio, N'Hugh Glass', CAST(1 AS bit), 1),
    (@Revenant, @Hardy, N'John Fitzgerald', CAST(0 AS bit), 2)
) AS v (MovieId, ActorId, CharacterName, IsLeadRole, BillingOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.MovieActors ma WHERE ma.MovieId = v.MovieId AND ma.ActorId = v.ActorId);

COMMIT TRANSACTION;

SELECT
    (SELECT COUNT(*) FROM dbo.Movies) AS Movies,
    (SELECT COUNT(*) FROM dbo.Actors) AS Actors,
    (SELECT COUNT(*) FROM dbo.MovieActors) AS MovieActors;
GO
