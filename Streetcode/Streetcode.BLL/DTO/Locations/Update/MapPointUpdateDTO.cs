using Streetcode.BLL.DTO.Interfaces;
using Streetcode.BLL.Enums;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Update;

namespace Streetcode.BLL.DTO.Locations.Update;

public class MapPointUpdateDTO : IModelState
{
    public int Id { get; set; }
    public int PlateNumber { get; set; }
    public string Address { get; set; } = null!;
    public int StreetcodeId { get; set; }
    public StreetcodeCoordinateUpdateDTO StreetcodeCoordinate { get; set; } = null!;
    public ModelState ModelState { get; set; }
}
