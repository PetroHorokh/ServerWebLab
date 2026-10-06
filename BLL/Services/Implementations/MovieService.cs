using AutoMapper;
using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BLL.Services.Implementations;

internal sealed class MovieService(
    IMovieRepository movieRepository,
    IActorRepository actorRepository,
    IMovieActorRepository movieActorRepository,
    AuditStamper auditStamper,
    IMapper mapper) : IMovieService
{
    private const string MissingActorsError =
        "One or more of the selected actors no longer exist. Reload the page and try again.";

    public async Task<List<MovieViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var movies = await movieRepository.GetAllWithActorsAsync(cancellationToken);
        return mapper.Map<List<MovieViewModel>>(movies);
    }

    public async Task<List<MovieSummaryViewModel>> GetSummariesAsync(CancellationToken cancellationToken = default)
    {
        var movies = await movieRepository.GetAllAsync(cancellationToken);
        return mapper.Map<List<MovieSummaryViewModel>>(movies);
    }

    public async Task<MovieViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdWithActorsAsync(id, cancellationToken);
        return movie is null ? null : mapper.Map<MovieViewModel>(movie);
    }

    public async Task<MovieDetailsViewModel?> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdWithActorsAsync(id, cancellationToken);
        return movie is null ? null : mapper.Map<MovieDetailsViewModel>(movie);
    }

    public async Task<MovieUpdateModel?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdWithActorsAsync(id, cancellationToken);
        return movie is null ? null : mapper.Map<MovieUpdateModel>(movie);
    }

    public async Task<ServiceResult> CreateAsync(MovieCreateModel model, CancellationToken cancellationToken = default)
    {
        var actorIds = model.ActorIds.Distinct().ToList();
        if (!await actorRepository.AllExistAsync(actorIds, cancellationToken))
        {
            return ServiceResult.Invalid(MissingActorsError);
        }

        var movie = auditStamper.StampCreated(mapper.Map<Movie>(model));
        foreach (var actorId in actorIds)
        {
            movie.MovieActors.Add(auditStamper.StampCreated(new MovieActor { ActorId = actorId }));
        }

        await movieRepository.AddAsync(movie, cancellationToken);
        await movieRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(MovieUpdateModel model, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdWithActorsAsync(model.Id, cancellationToken);
        if (movie is null)
        {
            return ServiceResult.NotFound();
        }

        var actorIds = model.ActorIds.ToHashSet();
        if (!await actorRepository.AllExistAsync(actorIds, cancellationToken))
        {
            return ServiceResult.Invalid(MissingActorsError);
        }

        mapper.Map(model, movie);
        auditStamper.StampModified(movie);

        var unselected = movie.MovieActors.Where(ma => !actorIds.Contains(ma.ActorId)).ToList();
        movieActorRepository.RemoveRange(unselected);

        var linkedActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToHashSet();
        foreach (var actorId in actorIds.Where(id => !linkedActorIds.Contains(id)))
        {
            movie.MovieActors.Add(auditStamper.StampCreated(new MovieActor { ActorId = actorId }));
        }

        await movieRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var movie = await movieRepository.GetByIdAsync(id, cancellationToken);
        if (movie is null)
        {
            return ServiceResult.NotFound();
        }

        movieRepository.Remove(movie);
        await movieRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
