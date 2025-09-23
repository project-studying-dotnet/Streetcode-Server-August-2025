using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.DAL.Entities.Favourite;

[Table("favourite_streetcodes", Schema = "favourite_streetcode")]
public class FavouriteStreetcode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int UserId { get; set; }
    public int StreetcodeId { get; set; }

    public User? User { get; set; }
    public StreetcodeContent? Streetcode { get; set; }
}
