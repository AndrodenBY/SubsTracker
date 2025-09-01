using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.API.ViewModel.User.Create;

namespace SubsTracker.API.Validators.User;

public class CreateUserViewModelValidator : AbstractValidator<CreateUserViewModel>
{
    public CreateUserViewModelValidator()
    {
        RuleFor(model => model.FirstName)
            .NotEmpty().WithMessage(ValidatorMessages.Required("FirstName"))
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length("FirstName"));
        
        RuleFor(model => model.LastName)
            .NotEmpty()
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .When(model => !string.IsNullOrWhiteSpace(model.LastName))
            .WithMessage(ValidatorMessages.Length("LastName"));
        
        RuleFor(model => model.Email)
            .NotEmpty().WithMessage(ValidatorMessages.Required("Email"))
            .EmailAddress().WithMessage(ValidatorMessages.Required("Valid Email"));
    }
}