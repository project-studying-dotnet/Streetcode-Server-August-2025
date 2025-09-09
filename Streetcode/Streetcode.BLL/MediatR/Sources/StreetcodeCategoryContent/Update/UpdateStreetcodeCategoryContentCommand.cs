using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update
{
    public record UpdateStreetcodeCategoryContentCommand(CategoryContentUpdateDTO CategoryContentUpdateDTO)
        : IRequest<Result<StreetcodeCategoryContentDTO>>;
}
