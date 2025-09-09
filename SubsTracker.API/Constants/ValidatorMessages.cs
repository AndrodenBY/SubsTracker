namespace SubsTracker.API.Constants;

public static class ValidatorMessages
{
    public static string Required(string fieldName) =>
        $"{fieldName} is required.";

    public static string CannotBeEmpty(string fieldName) =>
        $"{fieldName} cannot be empty.";

    public static string MustBeValid(string fieldName) =>
        $"Invalid {fieldName.ToLower()} value.";

    public static string MustBeSpecified(string fieldName) =>
        $"{fieldName} must be specified.";

    public static string Length(string fieldName) =>
        $"{fieldName} must be between {ValidatorConstants.MinimumNameLength} and {ValidatorConstants.MaximumNameLength} characters.";

    public static string MaxLength(string fieldName) =>
        $"{fieldName} cannot exceed {ValidatorConstants.MaximumNameLength} characters.";

    public static string PositiveValue(string fieldName) =>
        $"{fieldName} must be a positive value.";
}
