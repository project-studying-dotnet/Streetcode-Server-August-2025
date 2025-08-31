using Streetcode.BLL.DTO.Media.Images;

namespace Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

public class FactCreateDto
{
    public string Title { get; set; }
    public int ImageId { get; set; }
    public string FactContent { get; set; }
    public ImageDTO? Image { get; set; }
}
