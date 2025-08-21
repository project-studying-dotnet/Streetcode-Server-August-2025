using Streetcode.BLL.DTO.AdditionalContent.Tag;

namespace Streetcode.BLL.DTO.Streetcode.Create
{
    public class StreetcodeCreateDTO : StreetcodeCreateUpdateDTO
    {
        public int ViewCount { get; set; }
        public IEnumerable<StreetcodeTagDTO>? Tags { get; set; } = new List<StreetcodeTagDTO>();
    }
}
