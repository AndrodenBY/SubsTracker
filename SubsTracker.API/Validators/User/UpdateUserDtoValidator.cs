using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.API.ViewModel.User.Update;
using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.API.Validators.User;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(UpdateUserDto.Id)));

        RuleFor(model => model.FirstName)
            .MaximumLength(ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length(nameof(UpdateUserDto.FirstName)))
            .When(model => !string.IsNullOrEmpty(nameof(UpdateUserDto.FirstName)));
        
        RuleFor(model => model.LastName)
            .MaximumLength(ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length(nameof(UpdateUserDto.LastName)))
            .When(model => !string.IsNullOrEmpty(model.LastName));

        RuleFor(model => model.Email)
            .EmailAddress()
            .WithMessage(ValidatorMessages.MustBeValid(nameof(UpdateUserDto.Email)))
            .When(model => !string.IsNullOrEmpty(model.Email));
    }
}