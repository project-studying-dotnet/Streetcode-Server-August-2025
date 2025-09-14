using Ardalis.Specification.EntityFrameworkCore;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Users;

namespace Streetcode.DAL.Repositories.Realizations.Users;

public class RefreshTokenRepository: RepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(StreetcodeDbContext context)
        : base(context)
    {
    }
}