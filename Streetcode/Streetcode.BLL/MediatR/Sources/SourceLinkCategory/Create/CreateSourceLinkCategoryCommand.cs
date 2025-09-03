using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create
{
    public record CreateSourceLinkCategoryCommand(SourceLinkCategoryCreateDTO sourceLinkCategoryCreateDTO) : IRequest<Result<SourceLinkCategoryDTO>>;
}
