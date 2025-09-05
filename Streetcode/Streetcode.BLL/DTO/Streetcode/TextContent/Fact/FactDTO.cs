using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Streetcode.TextContent.Fact
{
    public class FactDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ImageId { get; set; }
        public string FactContent { get; set; }
        public int Order { get; set; }
    }
}
