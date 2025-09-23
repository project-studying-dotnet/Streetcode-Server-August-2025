using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streetcode.DAL.Entities.Favourite;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.FavouriteStreetcodes;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.DAL.Repositories.Realizations.FavouriteStreetcodes
{
    public class FavouriteStreetcodeRepository : Base.RepositoryBase<FavouriteStreetcode>, IFavouriteStreetcodeRepository
    {
        public FavouriteStreetcodeRepository(StreetcodeDbContext context)
            : base(context)
        {
        }
    }
}
