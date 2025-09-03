using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Locations;
using Streetcode.BLL.DTO.Locations.Update;

namespace Streetcode.BLL.DTO.Streetcode.Update;

public class StreetcodeUpdateDTO : StreetcodeCreateUpdateDTO
{
    public int Id { get; set; }
    public IEnumerable<StreetcodeTagUpdateDTO>? Tags { get; set; } = new List<StreetcodeTagUpdateDTO>();
    public IEnumerable<ImageUpdateDTO> Images { get; set; } = new List<ImageUpdateDTO>();
    public IEnumerable<MapPointUpdateDTO>? MapPoints { get; set; } = new List<MapPointUpdateDTO>();
}
