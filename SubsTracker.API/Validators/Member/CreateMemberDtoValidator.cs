using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.User.Create;

namespace SubsTracker.API.Validators.Member;

[ExcludeFromCodeCoverage]
public class CreateMemberDtoValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberDtoValidator()
    {
        RuleFor(model => model.UserId)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(CreateMemberDto.UserId)));

        RuleFor(model => model.GroupId)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(CreateMemberDto.GroupId)));

        RuleFor(model => model.Role)
            .IsInEnum()
            .WithMessage(ValidatorMessages.MustBeValid(nameof(CreateMemberDto.Role)));
    }
}
