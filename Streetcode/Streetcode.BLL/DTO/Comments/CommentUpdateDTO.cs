using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Comments;

public class CommentUpdateDTO
{
    public int Id { get; set; }
    public string Text { get; set; }
}
