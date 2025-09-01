using Streetcode.BLL.DTO.Interfaces;
using Streetcode.BLL.Enums;

namespace Streetcode.BLL.DTO.Media.Art;

public class ArtCreateUpdateDTO : IModelState
{
    public int Id { get; set; }
    public string Description { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int ImageId { get; set; }
    public ModelState ModelState { get; set; }
}
