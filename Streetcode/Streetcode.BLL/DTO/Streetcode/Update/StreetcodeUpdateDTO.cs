using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Locations;

namespace Streetcode.BLL.DTO.Streetcode.Update;

public class StreetcodeUpdateDTO : StreetcodeCreateUpdateDTO
{
    public int Id { get; set; }
    public IEnumerable<StreetcodeTagUpdateDTO>? Tags { get; set; } = new List<StreetcodeTagUpdateDTO>();
    public IEnumerable<ImageUpdateDTO> Images { get; set; } = new List<ImageUpdateDTO>();
    public IEnumerable<MapPointDTO>? MapPoint { get; set; } = new List<MapPointDTO>();
}
