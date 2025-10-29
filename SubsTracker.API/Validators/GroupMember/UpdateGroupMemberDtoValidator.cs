using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.API.Validators.GroupMember;

public class UpdateGroupMemberDtoValidator : AbstractValidator<UpdateGroupMemberDto>
{
    public UpdateGroupMemberDtoValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(UpdateGroupMemberDto.Id)));

        RuleFor(model => model.Role)
            .IsInEnum().When(model => model.Role.HasValue)
            .WithMessage(ValidatorMessages.MustBeValid(nameof(UpdateGroupMemberDto.Role)));
    }
}