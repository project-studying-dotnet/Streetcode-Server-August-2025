using Streetcode.BLL.DTO.Streetcode.TextContent;

namespace Streetcode.BLL.Interfaces.Terms
{
    public interface ITermService
    {
        Task<IEnumerable<TermDTO>> GetAllTermsAsync();
        Task<TermDTO> GetTermByIdAsync(int id);
        Task<TermDTO> CreateTermAsync(CreateTermDTO termDTO);
        Task<TermDTO> UpdateTermAsync(int id, CreateTermDTO termDTO);
        Task DeleteTermAsync(int id);
    }
}
