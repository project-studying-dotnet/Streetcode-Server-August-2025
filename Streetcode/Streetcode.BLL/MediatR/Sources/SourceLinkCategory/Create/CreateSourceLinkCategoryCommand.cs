using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create
{
    public record CreateSourceLinkCategoryCommand(SourceLinkCategoryCreateDTO SourceLinkCategoryCreateDTO)
        : IRequest<Result<SourceLinkCategoryDTO>>;
}
