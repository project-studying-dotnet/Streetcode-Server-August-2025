using AutoMapper;
using Streetcode.BLL.DTO.Comments;
using Streetcode.DAL.Entities.Comments;

namespace Streetcode.BLL.Mapping.Comments;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<CommentContent, CommentDTO>();

        CreateMap<CommentDTO, CommentContent>()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Streetcode, opt => opt.Ignore())
            .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
            .MaxDepth(3);

        CreateMap<CommentCreateDTO, CommentContent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Streetcode, opt => opt.Ignore())
            .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
            .ForMember(dest => dest.Replies, opt => opt.Ignore());

        CreateMap<CommentUpdateDTO, CommentContent>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.StreetcodeId, opt => opt.Ignore())
            .ForMember(dest => dest.ParentCommentId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Streetcode, opt => opt.Ignore())
            .ForMember(dest => dest.ParentComment, opt => opt.Ignore())
            .ForMember(dest => dest.Replies, opt => opt.Ignore());
    }
}
