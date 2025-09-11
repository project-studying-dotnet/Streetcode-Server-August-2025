using FluentValidation;
using Streetcode.BLL.DTO.Streetcode.Update;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.AdditionalContent.Tag;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Validators.Streetcode
{
    public class UpdateStreetcodeValidator : AbstractValidator<UpdateStreetcodeCommand>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UpdateStreetcodeValidator(
            IRepositoryWrapper repositoryWrapper,
            BaseStreetcodeValidator baseStreetcodeValidator,
            TagValidator tagValidator)
        {
            _repositoryWrapper = repositoryWrapper;

            RuleFor(c => c.Streetcode).SetValidator(baseStreetcodeValidator);

            RuleFor(c => c.Streetcode)
                .MustAsync(BeUniqueIndex)
                .WithMessage(Errors_Validation.MustBeUnique.FormatWith("Index"));

            RuleForEach(c => c.Streetcode.Tags).SetValidator(tagValidator);
        }

        private async Task<bool> BeUniqueIndex(StreetcodeUpdateDTO streetcode, CancellationToken cancellationToken)
        {
            var existingStreetcode = await _repositoryWrapper.StreetcodeRepository
                .GetFirstOrDefaultAsync(sc => sc.Index == streetcode.Index && sc.Id != streetcode.Id);

            return existingStreetcode == null;
        }
    }
}
