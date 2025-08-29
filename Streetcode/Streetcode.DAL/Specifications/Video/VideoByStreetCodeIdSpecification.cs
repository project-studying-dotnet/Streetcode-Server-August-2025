using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Streetcode.DAL.Specifications.Video
{
    public class VideoByStreetCodeIdSpecification : Specification<Entities.Media.Video>
    {
        public VideoByStreetCodeIdSpecification(int streetcodeId)
        {
            Query.Where(v => v.StreetcodeId == streetcodeId);
        }
    }
}