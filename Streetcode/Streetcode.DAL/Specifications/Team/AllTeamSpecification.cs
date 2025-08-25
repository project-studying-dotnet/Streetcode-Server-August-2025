using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace Streetcode.DAL.Specifications.Team
{
    public class AllTeamSpecification : Specification<Entities.Team.TeamMember>
    {
        public AllTeamSpecification()
        {
         Query.Include(x => x.Positions)
             .Include(x => x.TeamMemberLinks);
        }
    }
}
