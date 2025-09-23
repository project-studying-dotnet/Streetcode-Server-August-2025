using Streetcode.DAL.Entities.Favourite;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.FavouriteStreetcodes;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.DAL.Repositories.Realizations.FavouriteStreetcodes
{
    public class FavouriteStreetcodeRepository : RepositoryBase<FavouriteStreetcode>, IFavouriteStreetcodeRepository
    {
        public FavouriteStreetcodeRepository(StreetcodeDbContext context)
            : base(context)
        {
        }
    }
}
