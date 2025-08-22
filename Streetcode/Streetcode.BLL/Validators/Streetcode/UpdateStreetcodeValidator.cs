using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Validators.Streetcode
{
    public class UpdateStreetcodeValidator : AbstractValidator<StreetcodeUpdateDTO>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UpdateStreetcodeValidator(IRepositoryWrapper repositoryWrapper, BaseStreetcodeValidator baseStreetcodeValidator)
        {
            _repositoryWrapper = repositoryWrapper;

            RuleFor(dto => dto).SetValidator(baseStreetcodeValidator);

            RuleFor(dto => dto)
                .MustAsync(BeUniqueIndex).WithMessage("Index must be unique.");
        }

        private async Task<bool> BeUniqueIndex(StreetcodeUpdateDTO streetcode, CancellationToken cancellationToken)
        {
            var existingStreetcode = await _repositoryWrapper.StreetcodeRepository
                .GetFirstOrDefaultAsync(sc => sc.Index == streetcode.Index && sc.Id != streetcode.Id);

            return existingStreetcode == null;
        }
    }
}
