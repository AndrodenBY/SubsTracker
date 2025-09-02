using FluentValidation;
using SubsTracker.API.Constants;
using SubsTracker.Domain.Enums;
using SubsTracker.API.ViewModel.Subscription;

namespace SubsTracker.API.Validators.Subscription;

public class UpdateSubscriptionDtoValidator : AbstractValidator<UpdateSubscriptionViewModel>
{
    public UpdateSubscriptionDtoValidator()
    {
        RuleFor(model => model.Id)
            .NotEmpty().WithMessage(ValidatorMessages.Required("Id"))
            .NotEqual(Guid.Empty).WithMessage(ValidatorMessages.CannotBeEmpty("Id"));

        RuleFor(model => model.Name)
            .NotEmpty()
            .WithMessage(ValidatorMessages.CannotBeEmpty("Name"))
            .Length(ValidatorConstants.MinimumNameLength, ValidatorConstants.MaximumNameLength)
            .WithMessage(ValidatorMessages.Length("Name"));

        RuleFor(model => model.Price)
            .GreaterThanOrEqualTo(ValidatorConstants.MinimumPrice)
            .When(model => model.Price.HasValue)
            .WithMessage(ValidatorMessages.PositiveValue("Price"));
        
        RuleFor(model => model.Type)
            .Must(type => Enum.IsDefined(typeof(SubscriptionType), type))
            .WithMessage(ValidatorMessages.MustBeValid("Subscription Type"));

        RuleFor(model => model.Content)
            .Must(content => Enum.IsDefined(typeof(SubscriptionContent), content))
            .WithMessage(ValidatorMessages.MustBeValid("Subscription Content"));
    }
}