using Streetcode.BLL.DTO.Interfaces;
using Streetcode.BLL.Enums;

namespace Streetcode.BLL.DTO.AdditionalContent.Tag;

public class StreetcodeTagUpdateDTO : StreetcodeTagDTO, IModelState
{
    public int StreetcodeId { get; set; }
    public ModelState ModelState { get; set; }
}
