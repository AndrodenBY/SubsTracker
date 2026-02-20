using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.User.Create;

namespace SubsTracker.API.Validators.Group;

[ExcludeFromCodeCoverage]
public class CreateGroupDtoValidator : AbstractValidator<CreateGroupDto>
{
    public CreateGroupDtoValidator()
    {
        RuleFor(model => model.Name)
            .NotEmpty().WithMessage(ValidatorMessages.Required(nameof(CreateGroupDto.Name)))
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length(nameof(CreateGroupDto.Name)));
    }
}
