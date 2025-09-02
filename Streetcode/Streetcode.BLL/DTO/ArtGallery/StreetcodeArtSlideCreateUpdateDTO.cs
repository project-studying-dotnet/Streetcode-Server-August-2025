using Streetcode.BLL.DTO.Interfaces;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Enums;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.ArtGallery;

public class StreetcodeArtSlideCreateUpdateDTO : IModelState
{
    public int Id { get; set; }
    public int Index { get; set; }
    public int? StreetcodeId { get; set; }
    public GallerySlideTemplate Template { get; set; }
    public IEnumerable<StreetcodeArtCreateUpdateDTO> StreetcodeArts { get; set; } = new List<StreetcodeArtCreateUpdateDTO>();
    public ModelState ModelState { get; set; }
}
