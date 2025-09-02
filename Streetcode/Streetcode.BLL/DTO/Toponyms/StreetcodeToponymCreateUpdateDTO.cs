using Streetcode.BLL.DTO.Interfaces;
using Streetcode.BLL.Enums;

namespace Streetcode.BLL.DTO.Toponyms;

public class StreetcodeToponymCreateUpdateDTO : IModelState
{
    public int StreetcodeId { get; set; }
    public int ToponymId { get; set; }
    public string StreetName { get; set; } = null!;
    public ModelState ModelState { get; set; } = ModelState.Updated;
}