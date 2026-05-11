using System.Diagnostics.CodeAnalysis;

namespace DiagCore.Core.Common;

/// <summary>
/// Outcome of a diagnostic operation. Diagnostic calls touch WMI, native APIs
/// and external processes — any of which can fail at any moment. Returning a
/// <see cref="DiagnosticResult{T}"/> instead of throwing keeps the failure
/// modes explicit at the call site and prevents a single broken WMI provider
/// from crashing the whole scan.
/// </summary>
public readonly record struct DiagnosticResult<T>
{
    private DiagnosticResult(bool isSuccess, T? value, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public string? ErrorMessage { get; }

    public Exception? Exception { get; }

    public static DiagnosticResult<T> Success(T value) =>
        new(true, value, null, null);

    public static DiagnosticResult<T> Failure(string errorMessage, Exception? exception = null) =>
        new(false, default, errorMessage, exception);

    public static DiagnosticResult<T> FromException(Exception exception, string? contextMessage = null)
    {
        var message = string.IsNullOrWhiteSpace(contextMessage)
            ? exception.Message
            : $"{contextMessage}: {exception.Message}";
        return new(false, default, message, exception);
    }

    /// <summary>
    /// Returns the value when successful, otherwise returns the supplied fallback.
    /// Useful for surfacing partial diagnostic data without short-circuiting the scan.
    /// </summary>
    public T ValueOr(T fallback) => IsSuccess ? Value! : fallback;
}
