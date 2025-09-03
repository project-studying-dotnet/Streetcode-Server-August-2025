using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Delete
{
    public record DeleteStreetcodeCategoryContentCommand(int id) : IRequest<Result<StreetcodeCategoryContentDTO>>;
}
