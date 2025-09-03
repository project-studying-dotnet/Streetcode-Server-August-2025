using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Org.BouncyCastle.Asn1.X509;
using Streetcode.BLL.MediatR.Instagram.GetAll;

namespace Streetcode.WebApi.Controllers.Locations
{
    public class MapPointController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return HandleResult(await Mediator.Send(new GetAllStatisticRecordsQuery()));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id)
        {
            return HandleResult(await Mediator.Send(new UpdateMapPointCommand(id)));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            return HandleResult(await Mediator.Send(new DeleteMapPointCommand(id)));
        }
    }
}
