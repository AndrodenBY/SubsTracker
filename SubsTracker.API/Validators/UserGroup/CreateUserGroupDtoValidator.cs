using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.API.ViewModel.User.Create;
using SubsTracker.BLL.DTOs.User.Create;

namespace SubsTracker.API.Validators.UserGroup;

public class CreateUserGroupDtoValidator : AbstractValidator<CreateUserGroupDto>
{
    public CreateUserGroupDtoValidator()
    {
        RuleFor(model => model.Name)
            .NotEmpty().WithMessage(ValidatorMessages.Required(nameof(CreateUserGroupDto.Name)))
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length(nameof(CreateUserGroupDto.Name)));
    }
}
