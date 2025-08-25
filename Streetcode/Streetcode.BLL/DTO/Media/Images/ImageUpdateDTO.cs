using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streetcode.BLL.DTO.Interfaces;
using Streetcode.BLL.Enums;

namespace Streetcode.BLL.DTO.Media.Images
{
    public class ImageUpdateDTO : IModelState
    {
        public int Id { get; set; }
        public int StreedcodeId { get; set; }
        public ModelState ModelState { get; set; }
    }
}
