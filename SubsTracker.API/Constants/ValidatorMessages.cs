namespace SubsTracker.API.Constants;

public static class ValidatorMessages
{
    public static string Required(string fieldName)
    {
        return $"{fieldName} is required.";
    }

    public static string CannotBeEmpty(string fieldName)
    {
        return $"{fieldName} cannot be empty.";
    }

    public static string MustBeValid(string fieldName)
    {
        return $"Invalid {fieldName.ToLower()} value.";
    }

    public static string MustBeSpecified(string fieldName)
    {
        return $"{fieldName} must be specified.";
    }

    public static string Length(string fieldName)
    {
        return
            $"{fieldName} must be between {ValidatorConstants.MinimumNameLength} and {ValidatorConstants.MaximumNameLength} characters.";
    }

    public static string MaxLength(string fieldName)
    {
        return $"{fieldName} cannot exceed {ValidatorConstants.MaximumNameLength} characters.";
    }

    public static string PositiveValue(string fieldName)
    {
        return $"{fieldName} must be a positive value.";
    }
}