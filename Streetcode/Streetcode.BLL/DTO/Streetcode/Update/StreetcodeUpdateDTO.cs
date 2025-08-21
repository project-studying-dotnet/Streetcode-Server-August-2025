using Streetcode.BLL.DTO.AdditionalContent.Tag;

namespace Streetcode.BLL.DTO.Streetcode.Update;

public class StreetcodeUpdateDTO : StreetcodeCreateUpdateDTO
{
    public int Id { get; set; }
    public IEnumerable<StreetcodeTagUpdateDTO>? Tags { get; set; } = new List<StreetcodeTagUpdateDTO>();
}
