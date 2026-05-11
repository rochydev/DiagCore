using DiagCore.Core.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace DiagCore.Core;

/// <summary>
/// Composition root for everything under <c>DiagCore.Core</c>. Keeps the App
/// host file from growing a registration line per service.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers every diagnostic service in the Core assembly.
    /// Services are registered as singletons because they hold no per-call
    /// state and each WMI scope is constructed inside the call itself.
    /// </summary>
    public static IServiceCollection AddDiagCoreDiagnostics(this IServiceCollection services)
    {
        services.AddSingleton<ISystemDiagnostics, SystemDiagnostics>();
        services.AddSingleton<IHardwareDiagnostics, HardwareDiagnostics>();
        services.AddSingleton<IStorageDiagnostics, StorageDiagnostics>();
        services.AddSingleton<INetworkDiagnostics, NetworkDiagnostics>();
        services.AddSingleton<ISecurityDiagnostics, SecurityDiagnostics>();
        services.AddSingleton<IProcessDiagnostics, ProcessDiagnostics>();
        services.AddSingleton<IEventLogDiagnostics, EventLogDiagnostics>();
        return services;
    }
}
