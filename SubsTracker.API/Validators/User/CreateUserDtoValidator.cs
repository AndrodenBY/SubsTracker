using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.User.Create;

namespace SubsTracker.API.Validators.User;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(model => model.FirstName)
            .NotEmpty().WithMessage(ValidatorMessages.Required(nameof(CreateUserDto.FirstName)))
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length(nameof(CreateUserDto.FirstName)));

        RuleFor(model => model.LastName)
            .NotEmpty()
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .When(model => !string.IsNullOrWhiteSpace(model.LastName))
            .WithMessage(ValidatorMessages.Length(nameof(CreateUserDto.LastName)));

        RuleFor(model => model.Email)
            .NotEmpty().WithMessage(ValidatorMessages.Required(nameof(CreateUserDto.Email)))
            .EmailAddress().WithMessage(ValidatorMessages.MustBeValid(nameof(CreateUserDto.Email)));
    }
}
