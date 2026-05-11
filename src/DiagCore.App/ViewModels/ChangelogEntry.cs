namespace DiagCore.App.ViewModels;

/// <summary>
/// Single release entry shown in the welcome window. Hardcoded for now;
/// will be sourced from CHANGELOG.md once Phase 5 (Velopack releases) lands.
/// </summary>
public sealed record ChangelogEntry(string Version, DateOnly Date, string[] Changes);

public static class ChangelogData
{
    /// <summary>
    /// Releases ordered newest first. The top entry is treated as the
    /// "current" version by the welcome flow.
    /// </summary>
    public static IReadOnlyList<ChangelogEntry> Entries { get; } = new ChangelogEntry[]
    {
        new(
            Version: "0.1.0",
            Date: new DateOnly(2026, 5, 11),
            Changes:
            [
                "Andamiaje de la solución: DiagCore.App (WPF .NET 10) + DiagCore.Core + DiagCore.Tests.",
                "Sistema de diseño oscuro con paleta propia, Inter y JetBrains Mono embebidas.",
                "Custom chrome (FluentWindow + Mica) con sidebar de iconos estilo RedEngine.",
                "Núcleo de 7 servicios de diagnóstico sobre WMI: sistema, hardware, almacenamiento, red, seguridad, procesos, eventos.",
                "80 tests unitarios sobre los parsers y mapeos.",
                "Vistas funcionales con datos reales: Inicio (dashboard + score), Hardware, Almacenamiento (SMART), Red (ping + IP pública opt-in), Seguridad (Defender + firewall).",
                "Ventana de bienvenida con changelog, créditos y resumen de privacidad.",
            ]),
    };
}
