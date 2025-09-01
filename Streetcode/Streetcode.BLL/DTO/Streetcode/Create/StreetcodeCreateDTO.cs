using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.Locations;
using Streetcode.BLL.DTO.Media.Images;

namespace Streetcode.BLL.DTO.Streetcode.Create
{
    public class StreetcodeCreateDTO : StreetcodeCreateUpdateDTO
    {
        public int ViewCount { get; set; }
        public IEnumerable<StreetcodeTagDTO>? Tags { get; set; } = new List<StreetcodeTagDTO>();
        public IEnumerable<StreetcodeCoordinateDTO>? Coordinates { get; set; } = new List<StreetcodeCoordinateDTO>();
        public IEnumerable<MapPointDTO>? MapPoint { get; set; } = new List<MapPointDTO>();
    }
}
