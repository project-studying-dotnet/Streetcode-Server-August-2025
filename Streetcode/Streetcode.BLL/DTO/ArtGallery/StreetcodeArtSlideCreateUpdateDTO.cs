using Streetcode.BLL.DTO.Media.Art;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.ArtGallery;

public class StreetcodeArtSlideCreateUpdateDTO
{
    public int Id { get; set; }
    public int Index { get; set; }
    public int? StreetcodeId { get; set; }
    public GallerySlideTemplate Template { get; set; }
    public IEnumerable<StreetcodeArtCreateUpdateDTO> StreetcodeArts { get; set; } = new List<StreetcodeArtCreateUpdateDTO>();
}
