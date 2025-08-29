using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Streetcode.DAL.Specifications.Streetcode
{
    public class StreetCodeByIdSpecification : Specification<Entities.Streetcode.StreetcodeContent>
    {
        public StreetCodeByIdSpecification(int id)
        {
            Query.Where(s => s.Id == id);
        }
    }
}
