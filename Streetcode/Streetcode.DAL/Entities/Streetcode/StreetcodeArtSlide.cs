using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Streetcode.DAL.Enums;

namespace Streetcode.DAL.Entities.Streetcode;

[Table("streetcode_art_slides", Schema = "streetcode")]
public class StreetcodeArtSlide
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int Index { get; set; }
    [Required]
    public GallerySlideTemplate Template { get; set; }

    [Required]
    public int StreetcodeId { get; set; }

    public StreetcodeContent? Streetcode { get; set; }
    public List<StreetcodeArt>? StreetcodeArts { get; set; }
}