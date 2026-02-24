using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.API.Validators.Group;

public class UpdateGroupDtoValidator : AbstractValidator<UpdateGroupDto>
{
    public UpdateGroupDtoValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(UpdateGroupDto.Id)));

        RuleFor(model => model.Name)
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .When(model => !string.IsNullOrEmpty(model.Name))
            .WithMessage(ValidatorMessages.Length(nameof(UpdateGroupDto.Name)));
    }
}
