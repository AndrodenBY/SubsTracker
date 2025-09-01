using FluentValidation;
using SubsTracker.API.ViewModel.Subscription;
using SubsTracker.API.Constants;
using SubsTracker.Domain.Enums;

namespace SubsTracker.API.Validators.Subscription;

public class CreateSubscriptionViewModelValidator : AbstractValidator<CreateSubscriptionViewModel>
{
    public CreateSubscriptionViewModelValidator()
    {
        RuleFor(model => model.Name)
            .NotEmpty().WithMessage(ValidatorMessages.Required("Name"))
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength).WithMessage("Name must be between 2 and 50 characters.");

        RuleFor(model => model.Price)
            .GreaterThanOrEqualTo(ValidatorConstants.MinimumPrice)
            .WithMessage(ValidatorMessages.PositiveValue("Price"));
        
        RuleFor(model => model.DueDate)
            .NotEmpty().WithMessage(ValidatorMessages.Required("DueDate"));

        RuleFor(model => model.Type)
            .IsInEnum().WithMessage(ValidatorMessages.MustBeValid("Subscription Type"))
            .NotEqual(SubscriptionType.None).WithMessage(ValidatorMessages.MustBeSpecified("Subscription Type"));

        RuleFor(model => model.Content)
            .IsInEnum().WithMessage(ValidatorMessages.MustBeValid("Subscription Content"))
            .NotEqual(SubscriptionContent.None).WithMessage(ValidatorMessages.MustBeSpecified("Subscription Content"));
    }
}