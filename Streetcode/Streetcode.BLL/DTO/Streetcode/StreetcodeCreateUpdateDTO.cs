using Streetcode.BLL.DTO.Media.Images;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.Streetcode;

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
    public string? Teaser { get; set; }
    public StreetcodeStatus Status { get; set; }
    public int? AudioId { get; set; }
    public IEnumerable<ImageDetailsDto> ImagesDetails { get; set; } = new List<ImageDetailsDto>();
}
