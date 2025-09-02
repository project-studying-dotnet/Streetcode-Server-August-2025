using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;

namespace Streetcode.BLL.DTO.Locations;

public class MapPointDTO
{
    public int PlateNumber { get; set; }
    public StreetcodeCoordinateDTO StreetcodeCoordinate { get; set; } = null!;
    public string Address { get; set; } = null!;
}
