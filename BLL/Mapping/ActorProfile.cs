using AutoMapper;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using DAL.Entities;

namespace BLL.Mapping;

public class ActorProfile : Profile
{
    public ActorProfile()
    {
        CreateMap<Actor, ActorViewModel>()
            .ForMember(
                d => d.Movies,
                o => o.MapFrom(s => s.MovieActors
                    .Select(ma => ma.Movie)
                    .OrderBy(m => m.ReleaseYear)
                    .ThenBy(m => m.Title)));

        CreateMap<Actor, ActorSummaryViewModel>();

        CreateMap<Actor, ActorUpdateModel>()
            .ForMember(d => d.MovieIds, o => o.MapFrom(s => s.MovieActors.Select(ma => ma.MovieId)));

        CreateMap<ActorCreateModel, Actor>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.MovieActors, o => o.Ignore())
            .IgnoreAuditFields();

        CreateMap<ActorUpdateModel, Actor>()
            .IncludeBase<ActorCreateModel, Actor>();
    }
}
