using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.API.Validators.Member;

[ExcludeFromCodeCoverage]
public class UpdateMemberDtoValidator : AbstractValidator<UpdateMemberDto>
{
    public UpdateMemberDtoValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(UpdateMemberDto.Id)));

        RuleFor(model => model.Role)
            .IsInEnum().When(model => model.Role.HasValue)
            .WithMessage(ValidatorMessages.MustBeValid(nameof(UpdateMemberDto.Role)));
    }
}
