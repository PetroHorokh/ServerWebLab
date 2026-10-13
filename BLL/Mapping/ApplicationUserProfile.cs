using AutoMapper;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using DAL.Entities;

namespace BLL.Mapping;

public class ApplicationUserProfile : Profile
{
    public ApplicationUserProfile()
    {
        CreateMap<ApplicationUser, UserViewModel>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email ?? string.Empty))
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.UserIdentity != null ? s.UserIdentity.DisplayName : s.UserName ?? string.Empty))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.UserIdentity != null ? s.UserIdentity.FirstName : string.Empty))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.UserIdentity != null ? s.UserIdentity.LastName : string.Empty))
            .ForMember(d => d.DateOfBirth, o => o.MapFrom(s => s.UserIdentity != null ? s.UserIdentity.DateOfBirth : null))
            .ForMember(d => d.RegisteredDateTime, o => o.MapFrom(s => s.UserIdentity != null
                ? DateTime.SpecifyKind(s.UserIdentity.CreatedDateTime, DateTimeKind.Utc)
                : (DateTime?)null))
            .ForMember(d => d.Role, o => o.Ignore())
            .ForMember(d => d.MessageCount, o => o.Ignore());

        CreateMap<ApplicationUser, UserUpdateModel>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email ?? string.Empty))
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.UserIdentity != null ? s.UserIdentity.DisplayName : string.Empty))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.UserIdentity != null ? s.UserIdentity.FirstName : string.Empty))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.UserIdentity != null ? s.UserIdentity.LastName : string.Empty))
            .ForMember(d => d.DateOfBirth, o => o.MapFrom(s => s.UserIdentity != null ? s.UserIdentity.DateOfBirth : null))
            .ForMember(d => d.Role, o => o.Ignore());
    }
}
