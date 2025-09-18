using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoriesByStreetcodeId;
using Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoryById;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Delete;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetCategoryContentByStreetcodeId;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Update;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Delete;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.GetAll;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;
using Streetcode.DAL.Enums;
using Streetcode.WebApi.Attributes;

namespace Streetcode.WebApi.Controllers.Source;

public class SourcesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAllNames()
    {
        return HandleResult(await Mediator.Send(new GetAllCategoryNamesQuery()));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        return HandleResult(await Mediator.Send(new GetAllCategoriesQuery()));
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator, UserRole.Moderator)]
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] SourceLinkCategoryCreateDTO sourceLinkCategoryCreateDTO)
    {
        return HandleResult(await Mediator.Send(new CreateSourceLinkCategoryCommand(sourceLinkCategoryCreateDTO)));
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator, UserRole.Moderator)]
    [HttpPut]
    public async Task<IActionResult> UpdateCategory([FromBody] SourceLinkCategoryUpdateDTO sourceLinkCategoryUpdateDTO)
    {
        return HandleResult(await Mediator.Send(new UpdateSourceLinkCategoryCommand(sourceLinkCategoryUpdateDTO)));
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteSourceLinkCategoryCommand(id)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetCategoryByIdQuery(id)));
    }

    [HttpGet("{categoryId:int}&{streetcodeId:int}")]
    public async Task<IActionResult> GetCategoryContentByStreetcodeId([FromRoute] int streetcodeId, [FromRoute] int categoryId)
    {
        return HandleResult(await Mediator.Send(new GetCategoryContentByStreetcodeIdQuery(streetcodeId, categoryId)));
    }

    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetCategoriesByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetCategoriesByStreetcodeIdQuery(streetcodeId)));
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator, UserRole.Moderator)]
    [HttpPost]
    public async Task<IActionResult> CreateStreetCodeCategoryContent([FromBody] CategoryContentCreateDTO categoryContentCreateDTO)
    {
        return HandleResult(await Mediator.Send(new CreateStreetcodeCategoryContentCommand(categoryContentCreateDTO)));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStreetCodeCategoryContent()
    {
        return HandleResult(await Mediator.Send(new GetAllStreetcodeCategoryContentQuery()));
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStreetcodeCategoryContent([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteStreetcodeCategoryContentCommand(id)));
    }

    [AuthorizeRoles(UserRole.MainAdministrator, UserRole.Administrator, UserRole.Moderator)]
    [HttpPut]
    public async Task<IActionResult> UpdateStreetcodeCategoryContent([FromBody] CategoryContentUpdateDTO categoryContentUpdateDTO)
    {
        return HandleResult(await Mediator.Send(new UpdateStreetcodeCategoryContentCommand(categoryContentUpdateDTO)));
    }
}
