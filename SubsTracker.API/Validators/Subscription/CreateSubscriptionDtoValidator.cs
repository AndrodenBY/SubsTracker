using FluentValidation;
using SubsTracker.API.ViewModel.Subscription;
using SubsTracker.API.Constants;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.Domain.Enums;

namespace SubsTracker.API.Validators.Subscription;

public class CreateSubscriptionDtoValidator : AbstractValidator<CreateSubscriptionDto>
{
    public CreateSubscriptionDtoValidator()
    {
        RuleFor(model => model.Name)
            .NotEmpty()
            .WithMessage(ValidatorMessages.Required(nameof(CreateSubscriptionDto.Name)))
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Required(nameof(CreateSubscriptionDto.Name)));

        RuleFor(model => model.Price)
            .GreaterThanOrEqualTo(ValidatorConstants.MinimumPrice)
            .WithMessage(ValidatorMessages.PositiveValue(nameof(CreateSubscriptionDto.Price)));
        
        RuleFor(model => model.DueDate)
            .NotNull()
            .WithMessage(ValidatorMessages.Required(nameof(CreateSubscriptionDto.DueDate)));

        RuleFor(model => model.Type)
            .IsInEnum().WithMessage(ValidatorMessages.MustBeValid(nameof(CreateSubscriptionDto.Type)))
            .NotEqual(SubscriptionType.None)
            .WithMessage(ValidatorMessages.MustBeSpecified(nameof(CreateSubscriptionDto.Type)));

        RuleFor(model => model.Content)
            .IsInEnum().WithMessage(ValidatorMessages.MustBeValid("Subscription Content"))
            .NotEqual(SubscriptionContent.None)
            .WithMessage(ValidatorMessages.MustBeSpecified("Subscription Content"));
    }
}
