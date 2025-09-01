using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.API.ViewModel.User.Update;

namespace SubsTracker.API.Validators.GroupMember;

public class UpdateGroupMemberViewModelValidator : AbstractValidator<UpdateGroupMemberViewModel>
{
    public UpdateGroupMemberViewModelValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty().WithMessage(ValidatorMessages.Required("Id"))
            .NotEqual(Guid.Empty).WithMessage(ValidatorMessages.CannotBeEmpty("Id"));

        RuleFor(model => model.Role)
            .IsInEnum().When(model => model.Role.HasValue).WithMessage(ValidatorMessages.MustBeValid("Role"));
    }
}