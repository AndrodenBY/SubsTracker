namespace SubsTracker.Domain.Exceptions;

public class PolicyViolationException(string message) : Exception(message);
