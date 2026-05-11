using System.Management;
using System.Runtime.Versioning;

namespace DiagCore.Core.Common;

/// <summary>
/// Thin convenience layer over <see cref="ManagementObjectSearcher"/>. Centralises
/// scope handling, exception handling and disposal so callers can focus on the
/// projection from a <see cref="ManagementObject"/> to a domain model.
/// </summary>
[SupportedOSPlatform("windows")]
public static class WmiQuery
{
    public const string CimV2 = @"\\.\root\cimv2";
    public const string WmiCimV2 = @"\\.\root\wmi";
    public const string SecurityCenter2 = @"\\.\root\SecurityCenter2";
    public const string StandardCimv2 = @"\\.\root\StandardCimv2";
    public const string MicrosoftWindowsStorage = @"\\.\root\Microsoft\Windows\Storage";

    /// <summary>
    /// Runs <paramref name="query"/> in <paramref name="scope"/>, projects each
    /// result through <paramref name="projector"/> and returns the materialised
    /// list. Each <see cref="ManagementObject"/> is disposed after projection.
    /// Failures from the WMI subsystem are folded into a failed result; the
    /// caller never sees an unhandled exception.
    /// </summary>
    public static DiagnosticResult<IReadOnlyList<T>> Query<T>(
        string scope,
        string query,
        Func<ManagementObject, T> projector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentNullException.ThrowIfNull(projector);

        try
        {
            using var searcher = new ManagementObjectSearcher(scope, query);
            using var results = searcher.Get();

            var list = new List<T>();
            foreach (ManagementBaseObject baseObject in results)
            {
                if (baseObject is not ManagementObject obj)
                {
                    baseObject.Dispose();
                    continue;
                }

                try
                {
                    list.Add(projector(obj));
                }
                finally
                {
                    obj.Dispose();
                }
            }

            return DiagnosticResult<IReadOnlyList<T>>.Success(list);
        }
        catch (Exception ex)
        {
            return DiagnosticResult<IReadOnlyList<T>>.FromException(
                ex,
                $"WMI query failed (scope={scope}, query={query})");
        }
    }

    /// <summary>Convenience overload that targets the default <see cref="CimV2"/> scope.</summary>
    public static DiagnosticResult<IReadOnlyList<T>> Query<T>(
        string query,
        Func<ManagementObject, T> projector)
        => Query(CimV2, query, projector);

    /// <summary>
    /// Returns the first projected value, or a failure if the query is empty.
    /// Most WMI classes that describe singleton facts of the machine (BIOS,
    /// motherboard, OS) only return a single instance.
    /// </summary>
    public static DiagnosticResult<T> First<T>(string scope, string query, Func<ManagementObject, T> projector)
    {
        var result = Query(scope, query, projector);
        if (result.IsFailure)
        {
            return DiagnosticResult<T>.Failure(result.ErrorMessage!, result.Exception);
        }
        if (result.Value.Count == 0)
        {
            return DiagnosticResult<T>.Failure(
                $"WMI query returned no rows (scope={scope}, query={query})");
        }
        return DiagnosticResult<T>.Success(result.Value[0]);
    }

    public static DiagnosticResult<T> First<T>(string query, Func<ManagementObject, T> projector)
        => First(CimV2, query, projector);

    /// <summary>
    /// Returns the value of a property, coerced to <typeparamref name="T"/>, or
    /// <paramref name="fallback"/> when the property is absent or null.
    /// </summary>
    public static T GetValue<T>(ManagementObject obj, string property, T fallback)
    {
        try
        {
            var raw = obj[property];
            return raw is null ? fallback : (T)Convert.ChangeType(raw, typeof(T))!;
        }
        catch
        {
            return fallback;
        }
    }

    /// <summary>String property accessor returning empty string when missing.</summary>
    public static string GetString(ManagementObject obj, string property)
    {
        try
        {
            return obj[property]?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
