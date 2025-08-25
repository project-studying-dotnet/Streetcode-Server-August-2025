using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace Streetcode.DAL.Specifications.Team
{
    public class TeamByIdSpecification : Specification<Entities.Team.TeamMember>
    {
        public TeamByIdSpecification(int id)
        {
            Query.Where(p => p.Id == id)
                 .Include(p => p.TeamMemberLinks)
                 .Include(p => p.Positions);
        }
    }
}
