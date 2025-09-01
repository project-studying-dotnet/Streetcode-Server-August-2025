using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;

namespace Streetcode.BLL.DTO.Locations;

public class MapPointResponseDTO
{
    public int Id { get; set; }
    public int PlateNumber { get; set; }
    public StreetcodeCoordinateDTO StreetcodeCoordinate { get; set; } = null!;
    public string Address { get; set; } = null!;
}