using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Services.Text.Fact
{
    public class FactAutoOrder
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public FactAutoOrder(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<int> SetOrderForFacts(int strID)
        {
            var facts = await _repositoryWrapper.FactRepository.GetAllAsync(f => f.StreetcodeId == strID);

            if (!facts.Any())
            {
                return 1;
            }

            return facts.Max(f => f.Order) + 1;
        }
    }
}
