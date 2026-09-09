namespace BiseSukkur.Core.Common;

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public string? Details { get; }

    public Error(string code, string message, string? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }

    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
    public static readonly Error NotFound = new("Error.NotFound", "The requested entity was not found.");
    public static readonly Error Unauthorized = new("Error.Unauthorized", "You are not authorized to perform this action.");
    public static readonly Error Validation = new("Error.Validation", "One or more validation errors occurred.");
    public static readonly Error Conflict = new("Error.Conflict", "A conflict occurred with existing data.");
}
