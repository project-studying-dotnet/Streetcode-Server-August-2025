using Streetcode.BLL.DTO.Media.Images;

namespace Streetcode.BLL.DTO.Streetcode.TextContent.Fact
{
    public class FactUpdateCreateDto : FactDto
    {
        public string? ImageDescription { get; set; }

        public ImageFileBaseCreateDTO? NewImage { get; set; }
    }
}