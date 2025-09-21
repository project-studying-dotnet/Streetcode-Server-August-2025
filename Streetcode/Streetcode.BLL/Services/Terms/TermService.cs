using AutoMapper;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Terms;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Services.Terms
{
    public class TermService : ITermService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public TermService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TermDTO>> GetAllTermsAsync()
        {
            var terms = await _repositoryWrapper.TermRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TermDTO>>(terms);
        }

        public async Task<TermDTO> GetTermByIdAsync(int id)
        {
            var term = await _repositoryWrapper.TermRepository.GetSingleOrDefaultAsync(t => t.Id == id);
            if (term is null)
            {
                throw new KeyNotFoundException($"Term with id {id} not found.");
            }

            return _mapper.Map<TermDTO>(term);
        }

        public async Task<TermDTO> CreateTermAsync(CreateTermDTO termDTO)
        {
            var newTerm = _mapper.Map<Term>(termDTO);
            _repositoryWrapper.TermRepository.Create(newTerm);
            await _repositoryWrapper.SaveChangesAsync();
            return _mapper.Map<TermDTO>(newTerm);
        }

        public async Task<TermDTO> UpdateTermAsync(int id, CreateTermDTO termDTO)
        {
            var termToUpdate = await _repositoryWrapper.TermRepository.GetSingleOrDefaultAsync(t => t.Id == id);
            if (termToUpdate == null)
            {
                throw new KeyNotFoundException($"Term with id {id} not found.");
            }

            _mapper.Map(termDTO, termToUpdate);
            _repositoryWrapper.TermRepository.Update(termToUpdate);
            await _repositoryWrapper.SaveChangesAsync();
            return _mapper.Map<TermDTO>(termToUpdate);
        }

        public async Task DeleteTermAsync(int id)
        {
            var termToDelete = await _repositoryWrapper.TermRepository.GetSingleOrDefaultAsync(t => t.Id == id);
            if (termToDelete == null)
            {
                throw new KeyNotFoundException($"Term with id {id} not found.");
            }

            _repositoryWrapper.TermRepository.Delete(termToDelete);
            await _repositoryWrapper.SaveChangesAsync();
        }
    }
}
