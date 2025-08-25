using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streetcode.BLL.Enums;

namespace Streetcode.BLL.DTO.Interfaces
{
    public interface IModelState
    {
        public ModelState ModelState { get; set; }
    }
}
