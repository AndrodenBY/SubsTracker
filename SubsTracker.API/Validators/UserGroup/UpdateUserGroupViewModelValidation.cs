using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.API.ViewModel.User.Update;

namespace SubsTracker.API.Validators.UserGroup;

public class UpdateUserGroupViewModelValidator : AbstractValidator<UpdateUserGroupViewModel>
{
    public UpdateUserGroupViewModelValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty().WithMessage(ValidatorMessages.Required("Id"))
            .NotEqual(Guid.Empty).WithMessage(ValidatorMessages.CannotBeEmpty("Id"));

        RuleFor(model => model.Name)
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .When(model => !string.IsNullOrEmpty(model.Name))
            .WithMessage(ValidatorMessages.Length("Name"));
    }
}