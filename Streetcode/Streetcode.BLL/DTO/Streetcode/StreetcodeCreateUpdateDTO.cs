using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.DAL.Enums;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.Locations;

namespace Streetcode.BLL.DTO.Streetcode;

public class StreetcodeCreateDTO : StreetcodeCreateUpdateDTO
{
    public int ViewCount { get; set; }
    public IEnumerable<StreetcodeTagDTO>? Tags { get; set; } = new List<StreetcodeTagDTO>();
    public IEnumerable<StreetcodeCoordinateDTO>? Coordinates { get; set; } = new List<StreetcodeCoordinateDTO>();
    public IEnumerable<MapPointDTO>? MapPoints { get; set; } = new List<MapPointDTO>();
}
public class StreetcodeCreateUpdateDTO
{
    public int Index { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Title { get; set; } = null!;
    public StreetcodeType StreetcodeType { get; set; }
    public string? Alias { get; set; }
    public string TransliterationUrl { get; set; } = null!;
    public DateTime EventStartOrPersonBirthDate { get; set; }
    public DateTime? EventEndOrPersonDeathDate { get; set; }
    public string DateString { get; set; } = null!;

    /// <summary>
    /// A short teaser text for the streetcode.
    /// Validation rules:
    /// - If the teaser contains one or more newline characters, the maximum length is 520 characters.
    /// - If the teaser does not contain any newline characters, the maximum length is 455 characters.
    /// </summary>
    public string? Teaser { get; set; }
    public StreetcodeStatus Status { get; set; }
    public int? AudioId { get; set; }
    public IEnumerable<ImageDetailsDto> ImagesDetails { get; set; } = new List<ImageDetailsDto>();
    public IEnumerable<StreetcodeToponymCreateUpdateDTO>? Toponyms { get; set; } = new List<StreetcodeToponymCreateUpdateDTO>();
    public IEnumerable<ArtCreateUpdateDTO> Arts { get; set; } = new List<ArtCreateUpdateDTO>();
    public IEnumerable<StreetcodeArtSlideCreateUpdateDTO> StreetcodeArtSlides { get; set; } = new List<StreetcodeArtSlideCreateUpdateDTO>();
}
