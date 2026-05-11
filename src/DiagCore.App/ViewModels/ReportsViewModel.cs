using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DiagCore.App.ViewModels;

/// <summary>
/// Reports section view model. Phase 4 (PDF generation, SQLite history, wizard
/// export) will land here. For now the view shows the wizard skeleton with
/// section toggles and a disabled export button so the user can see where the
/// feature is going.
/// </summary>
public sealed partial class ReportsViewModel : ObservableObject
{
    public ReportsViewModel()
    {
        Sections =
        [
            new ReportSection("hardware", "Hardware (CPU, RAM, GPU, BIOS)", true),
            new ReportSection("storage",  "Almacenamiento (volúmenes y SMART)", true),
            new ReportSection("network",  "Red (adaptadores, IPs)", true),
            new ReportSection("security", "Seguridad (Defender, firewall)", true),
            new ReportSection("processes","Procesos y servicios", false),
            new ReportSection("events",   "Eventos críticos 24h", true),
            new ReportSection("hotfixes", "Actualizaciones instaladas", false),
        ];
    }

    public ObservableCollection<ReportSection> Sections { get; }
}

public partial class ReportSection : ObservableObject
{
    public ReportSection(string key, string label, bool include)
    {
        Key = key;
        Label = label;
        Include = include;
    }

    public string Key { get; }

    public string Label { get; }

    [ObservableProperty]
    private bool _include;
}
