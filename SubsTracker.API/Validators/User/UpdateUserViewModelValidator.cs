using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.API.ViewModel.User.Update;

namespace SubsTracker.API.Validators.User;

public class UpdateUserViewModelValidator : AbstractValidator<UpdateUserViewModel>
{
    public UpdateUserViewModelValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required("Id"))
            .NotEqual(Guid.Empty)
            .WithMessage(ValidatorMessages.CannotBeEmpty("Id"));

        RuleFor(model => model.FirstName)
            .MaximumLength(ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length("FirstName"))
            .When(model => !string.IsNullOrEmpty(model.FirstName));
        
        RuleFor(model => model.LastName)
            .MaximumLength(ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length("LastName"))
            .When(model => !string.IsNullOrEmpty(model.LastName));

        RuleFor(model => model.Email)
            .EmailAddress()
            .WithMessage(ValidatorMessages.MustBeValid("Email"))
            .When(model => !string.IsNullOrEmpty(model.Email));
    }
}