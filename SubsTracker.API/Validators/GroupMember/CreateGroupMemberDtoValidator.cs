using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.User.Create;

namespace SubsTracker.API.Validators.GroupMember;

public class CreateGroupMemberDtoValidator : AbstractValidator<CreateGroupMemberDto>
{
    public CreateGroupMemberDtoValidator()
    {
        RuleFor(model => model.UserId)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(CreateGroupMemberDto.UserId)));

        RuleFor(model => model.GroupId)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(CreateGroupMemberDto.GroupId)));

        RuleFor(model => model.Role)
            .IsInEnum()
            .WithMessage(ValidatorMessages.MustBeValid(nameof(CreateGroupMemberDto.Role)));
    }
}