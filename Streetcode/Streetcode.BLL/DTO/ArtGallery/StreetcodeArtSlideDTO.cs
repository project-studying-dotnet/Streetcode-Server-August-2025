using Streetcode.BLL.DTO.Media.Art;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.ArtGallery;

public class StreetcodeArtSlideDTO
{
    public int Id { get; set; }
    public int Index { get; set; }
    public GallerySlideTemplate Template { get; set; }
    public int StreetcodeId { get; set; }
    public List<StreetcodeArtDTO> StreetcodeArts { get; set; } = new();
}
