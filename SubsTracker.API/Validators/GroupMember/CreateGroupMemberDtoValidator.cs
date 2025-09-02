using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.API.ViewModel.User.Create; 

namespace SubsTracker.API.Validators.GroupMember;

public class CreateGroupMemberDtoValidator : AbstractValidator<CreateGroupMemberViewModel>
{
    public CreateGroupMemberDtoValidator()
    {
        RuleFor(model => model.UserId)
            .NotEmpty().WithMessage(ValidatorMessages.Required("UserId"))
            .NotEqual(Guid.Empty).WithMessage(ValidatorMessages.CannotBeEmpty("UserId"));

        RuleFor(model => model.GroupId)
            .NotEmpty().WithMessage(ValidatorMessages.Required("GroupId"))
            .NotEqual(Guid.Empty).WithMessage(ValidatorMessages.CannotBeEmpty("GroupId"));

        RuleFor(model => model.Role)
            .IsInEnum().WithMessage(ValidatorMessages.MustBeValid("Role"));
    }
}