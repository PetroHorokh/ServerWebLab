using AutoMapper;
using BLL.Common;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;

namespace BLL.Services.Implementations;

internal sealed class MovieActorService(
    IMovieActorRepository movieActorRepository,
    IMovieRepository movieRepository,
    IActorRepository actorRepository,
    AuditStamper auditStamper,
    IMapper mapper) : IMovieActorService
{
    public async Task<List<MovieActorViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await movieActorRepository.GetAllWithDetailsAsync(cancellationToken);
        return mapper.Map<List<MovieActorViewModel>>(roles);
    }

    public async Task<MovieActorViewModel?> GetAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default)
    {
        var role = await movieActorRepository.GetAsync(movieId, actorId, cancellationToken);
        return role is null ? null : mapper.Map<MovieActorViewModel>(role);
    }

    public async Task<MovieActorUpdateModel?> GetForUpdateAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default)
    {
        var role = await movieActorRepository.GetAsync(movieId, actorId, cancellationToken);
        return role is null ? null : mapper.Map<MovieActorUpdateModel>(role);
    }

    public async Task<ServiceResult> CreateAsync(MovieActorCreateModel model, CancellationToken cancellationToken = default)
    {
        if (model.MovieId is not { } movieId || model.ActorId is not { } actorId)
        {
            return ServiceResult.Invalid("Select both a movie and an actor.");
        }

        if (!await movieRepository.ExistsAsync(movieId, cancellationToken))
        {
            return ServiceResult.Invalid("The selected movie no longer exists. Reload the page and try again.");
        }

        if (!await actorRepository.ExistsAsync(actorId, cancellationToken))
        {
            return ServiceResult.Invalid("The selected actor no longer exists. Reload the page and try again.");
        }

        if (await movieActorRepository.ExistsAsync(movieId, actorId, cancellationToken))
        {
            return ServiceResult.Invalid("This actor already has a role in the selected movie. Edit that role instead.");
        }

        var role = auditStamper.StampCreated(mapper.Map<MovieActor>(model));
        await movieActorRepository.AddAsync(role, cancellationToken);
        await movieActorRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(MovieActorUpdateModel model, CancellationToken cancellationToken = default)
    {
        if (model.MovieId is not { } movieId || model.ActorId is not { } actorId)
        {
            return ServiceResult.NotFound();
        }

        var role = await movieActorRepository.GetAsync(movieId, actorId, cancellationToken);
        if (role is null)
        {
            return ServiceResult.NotFound();
        }

        mapper.Map(model, role);
        auditStamper.StampModified(role);
        await movieActorRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid movieId, Guid actorId, CancellationToken cancellationToken = default)
    {
        var role = await movieActorRepository.GetAsync(movieId, actorId, cancellationToken);
        if (role is null)
        {
            return ServiceResult.NotFound();
        }

        movieActorRepository.Remove(role);
        await movieActorRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
