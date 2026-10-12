using AutoMapper;
using BLL.Models.Actions;
using BLL.Models.ViewModels;
using DAL.Entities;

namespace BLL.Mapping;

public class ChatMessageProfile : Profile
{
    public ChatMessageProfile()
    {
        CreateMap<ChatMessage, ChatMessageViewModel>()
            .ForMember(d => d.AuthorId, o => o.MapFrom(s => s.UserId))
            .ForMember(d => d.AuthorName, o => o.MapFrom(s => s.User.UserIdentity != null ? s.User.UserIdentity.DisplayName : s.User.UserName))
            .ForMember(d => d.PostedDateTime, o => o.MapFrom(s => DateTime.SpecifyKind(s.PostedDateTime, DateTimeKind.Utc)));

        CreateMap<ChatMessageCreateModel, ChatMessage>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.MovieId, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.Message, o => o.MapFrom(s => s.Message.Trim()))
            .ForMember(d => d.PostedDateTime, o => o.Ignore())
            .ForMember(d => d.RoleTag, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Movie, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .IgnoreAuditFields();
    }
}
