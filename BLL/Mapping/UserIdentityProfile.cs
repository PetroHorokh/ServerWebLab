using AutoMapper;
using BLL.Common;
using BLL.Models.Actions;
using DAL.Entities;

namespace BLL.Mapping;

public class UserIdentityProfile : Profile
{
    public UserIdentityProfile()
    {
        CreateMap<RegisterModel, UserIdentity>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.DisplayName.Trim()))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName.Trim()))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName.Trim()))
            .IgnoreAuditFields();

        CreateMap<DefaultAdminOptions, UserIdentity>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.DateOfBirth, o => o.Ignore())
            .IgnoreAuditFields();
    }
}
