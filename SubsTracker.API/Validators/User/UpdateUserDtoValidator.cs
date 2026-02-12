using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.API.Validators.User;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(model => model.FirstName)
            .MaximumLength(ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length(nameof(UpdateUserDto.FirstName)))
            .When(_ => !string.IsNullOrEmpty(nameof(UpdateUserDto.FirstName)));

        RuleFor(model => model.LastName)
            .MaximumLength(ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length(nameof(UpdateUserDto.LastName)))
            .When(model => !string.IsNullOrEmpty(model.LastName));
    }
}
