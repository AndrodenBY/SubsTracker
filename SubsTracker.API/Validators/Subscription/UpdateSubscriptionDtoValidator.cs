using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.Domain.Enums;

namespace SubsTracker.API.Validators.Subscription;

public class UpdateSubscriptionDtoValidator : AbstractValidator<UpdateSubscriptionDto>
{
    public UpdateSubscriptionDtoValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(UpdateSubscriptionDto.Id)));

        RuleFor(model => model.Name)
            .NotEmpty()
            .WithMessage(ValidatorMessages.CannotBeEmpty(nameof(UpdateSubscriptionDto.Name)))
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length(nameof(UpdateSubscriptionDto.Name)));

        RuleFor(model => model.Price)
            .GreaterThanOrEqualTo(ValidatorConstants.MinimumPrice)
            .When(model => model.Price.HasValue)
            .WithMessage(ValidatorMessages.PositiveValue(nameof(UpdateSubscriptionDto.Price)));

        RuleFor(model => model.Type)
            .Must(type => Enum.IsDefined(typeof(SubscriptionType), type))
            .WithMessage(ValidatorMessages.MustBeValid(nameof(UpdateSubscriptionDto.Type)));

        RuleFor(model => model.Content)
            .Must(content => Enum.IsDefined(typeof(SubscriptionContent), content))
            .WithMessage(ValidatorMessages.MustBeValid(nameof(UpdateSubscriptionDto.Content)));
    }
}
