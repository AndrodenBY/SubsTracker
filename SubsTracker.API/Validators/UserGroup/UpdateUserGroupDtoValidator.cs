using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.API.Validators.UserGroup;

public class UpdateUserGroupDtoValidator : AbstractValidator<UpdateUserGroupDto>
{
    public UpdateUserGroupDtoValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(UpdateUserGroupDto.Id)));

        RuleFor(model => model.Name)
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .When(model => !string.IsNullOrEmpty(model.Name))
            .WithMessage(ValidatorMessages.Length(nameof(UpdateUserGroupDto.Name)));
    }
}