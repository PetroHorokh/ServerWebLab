using AutoMapper;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using DAL.Entities;

namespace BLL.Mapping;

public class MovieProfile : Profile
{
    public MovieProfile()
    {
        CreateMap<Movie, MovieViewModel>()
            .ForMember(
                d => d.Actors,
                o => o.MapFrom(s => s.MovieActors
                    .OrderByDescending(ma => ma.IsLeadRole)
                    .ThenBy(ma => ma.BillingOrder ?? int.MaxValue)
                    .ThenBy(ma => ma.Actor.LastName)
                    .Select(ma => ma.Actor)));

        CreateMap<Movie, MovieDetailsViewModel>()
            .IncludeBase<Movie, MovieViewModel>()
            .ForMember(
                d => d.Cast,
                o => o.MapFrom(s => s.MovieActors
                    .OrderByDescending(ma => ma.IsLeadRole)
                    .ThenBy(ma => ma.BillingOrder ?? int.MaxValue)
                    .ThenBy(ma => ma.Actor.LastName)));

        CreateMap<Movie, MovieSummaryViewModel>();

        CreateMap<Movie, MovieUpdateModel>()
            .ForMember(d => d.ActorIds, o => o.MapFrom(s => s.MovieActors.Select(ma => ma.ActorId)));

        CreateMap<MovieCreateModel, Movie>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.MovieActors, o => o.Ignore())
            .ForMember(d => d.ChatMessages, o => o.Ignore())
            .IgnoreAuditFields();

        CreateMap<MovieUpdateModel, Movie>()
            .IncludeBase<MovieCreateModel, Movie>();
    }
}
