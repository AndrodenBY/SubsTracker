using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.API.ViewModel.User.Create;

namespace SubsTracker.API.Validators.UserGroup;

public class CreateUserGroupViewModelValidator : AbstractValidator<CreateUserGroupViewModel>
{
    public CreateUserGroupViewModelValidator()
    {
        RuleFor(model => model.Name)
            .NotEmpty().WithMessage(ValidatorMessages.Required("Name"))
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length("Name"));
    }
}