using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.Update;
using Streetcode.BLL.Validators.AdditionalContent.Tag;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Validators.Streetcode
{
    public class UpdateStreetcodeValidator : AbstractValidator<StreetcodeUpdateDTO>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UpdateStreetcodeValidator(
            IRepositoryWrapper repositoryWrapper,
            BaseStreetcodeValidator baseStreetcodeValidator,
            TagValidator tagValidator)
        {
            _repositoryWrapper = repositoryWrapper;

            RuleFor(dto => dto).SetValidator(baseStreetcodeValidator);

            RuleFor(dto => dto)
                .MustAsync(BeUniqueIndex).WithMessage("Index must be unique.");

            RuleForEach(dto => dto.Tags).SetValidator(tagValidator);
        }

        private async Task<bool> BeUniqueIndex(StreetcodeUpdateDTO streetcode, CancellationToken cancellationToken)
        {
            var existingStreetcode = await _repositoryWrapper.StreetcodeRepository
                .GetFirstOrDefaultAsync(sc => sc.Index == streetcode.Index && sc.Id != streetcode.Id);

            return existingStreetcode == null;
        }
    }
}
