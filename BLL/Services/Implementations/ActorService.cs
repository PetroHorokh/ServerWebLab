using AutoMapper;
using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BLL.Services.Implementations;

internal sealed class ActorService(
    IActorRepository actorRepository,
    IMovieRepository movieRepository,
    IMovieActorRepository movieActorRepository,
    AuditStamper auditStamper,
    IMapper mapper) : IActorService
{
    private const string MissingMoviesError =
        "One or more of the selected movies no longer exist. Reload the page and try again.";

    public async Task<List<ActorViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var actors = await actorRepository.GetAllWithMoviesAsync(cancellationToken);
        return mapper.Map<List<ActorViewModel>>(actors);
    }

    public async Task<List<ActorSummaryViewModel>> GetSummariesAsync(CancellationToken cancellationToken = default)
    {
        var actors = await actorRepository.GetAllAsync(cancellationToken);
        return mapper.Map<List<ActorSummaryViewModel>>(actors);
    }

    public async Task<ActorViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await actorRepository.GetByIdWithMoviesAsync(id, cancellationToken);
        return actor is null ? null : mapper.Map<ActorViewModel>(actor);
    }

    public async Task<ActorUpdateModel?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await actorRepository.GetByIdWithMoviesAsync(id, cancellationToken);
        return actor is null ? null : mapper.Map<ActorUpdateModel>(actor);
    }

    public async Task<ServiceResult> CreateAsync(ActorCreateModel model, CancellationToken cancellationToken = default)
    {
        var movieIds = model.MovieIds.Distinct().ToList();
        if (!await movieRepository.AllExistAsync(movieIds, cancellationToken))
        {
            return ServiceResult.Invalid(MissingMoviesError);
        }

        var actor = auditStamper.StampCreated(mapper.Map<Actor>(model));
        foreach (var movieId in movieIds)
        {
            actor.MovieActors.Add(auditStamper.StampCreated(new MovieActor { MovieId = movieId }));
        }

        await actorRepository.AddAsync(actor, cancellationToken);
        await actorRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(ActorUpdateModel model, CancellationToken cancellationToken = default)
    {
        var actor = await actorRepository.GetByIdWithMoviesAsync(model.Id, cancellationToken);
        if (actor is null)
        {
            return ServiceResult.NotFound();
        }

        var movieIds = model.MovieIds.ToHashSet();
        if (!await movieRepository.AllExistAsync(movieIds, cancellationToken))
        {
            return ServiceResult.Invalid(MissingMoviesError);
        }

        mapper.Map(model, actor);
        auditStamper.StampModified(actor);

        var unselected = actor.MovieActors.Where(ma => !movieIds.Contains(ma.MovieId)).ToList();
        movieActorRepository.RemoveRange(unselected);

        var linkedMovieIds = actor.MovieActors.Select(ma => ma.MovieId).ToHashSet();
        foreach (var movieId in movieIds.Where(id => !linkedMovieIds.Contains(id)))
        {
            actor.MovieActors.Add(auditStamper.StampCreated(new MovieActor { MovieId = movieId }));
        }

        await actorRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actor = await actorRepository.GetByIdAsync(id, cancellationToken);
        if (actor is null)
        {
            return ServiceResult.NotFound();
        }

        actorRepository.Remove(actor);
        await actorRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
