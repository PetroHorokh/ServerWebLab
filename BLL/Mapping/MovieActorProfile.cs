using AutoMapper;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using DAL.Entities;

namespace BLL.Mapping;

public class MovieActorProfile : Profile
{
    public MovieActorProfile()
    {
        CreateMap<MovieActor, MovieActorViewModel>();

        CreateMap<MovieActor, CastMemberViewModel>();

        CreateMap<MovieActor, MovieActorUpdateModel>();

        CreateMap<MovieActorCreateModel, MovieActor>()
            .ForMember(d => d.Movie, o => o.Ignore())
            .ForMember(d => d.Actor, o => o.Ignore())
            .IgnoreAuditFields();

        CreateMap<MovieActorUpdateModel, MovieActor>()
            .IncludeBase<MovieActorCreateModel, MovieActor>()
            .ForMember(d => d.MovieId, o => o.Ignore())
            .ForMember(d => d.ActorId, o => o.Ignore());
    }
}
