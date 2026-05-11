using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiagCore.App.Controls;

/// <summary>
/// Severity used by <see cref="StatusBadge"/> to pick its colour scheme.
/// </summary>
public enum BadgeVariant
{
    Ok,
    Warning,
    Danger,
    Info,
    Neutral,
}

/// <summary>
/// Small rounded pill with a coloured dot + label. Used to surface boolean
/// or enum-valued status (SMART Healthy, Defender ON, UEFI, etc.).
/// </summary>
public partial class StatusBadge : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(StatusBadge), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(BadgeVariant), typeof(StatusBadge),
        new PropertyMetadata(BadgeVariant.Neutral, OnVariantChanged));

    public static readonly DependencyProperty ForegroundBrushProperty = DependencyProperty.Register(
        nameof(ForegroundBrush), typeof(Brush), typeof(StatusBadge),
        new PropertyMetadata(default(Brush)));

    public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register(
        nameof(BackgroundBrush), typeof(Brush), typeof(StatusBadge),
        new PropertyMetadata(default(Brush)));

    public StatusBadge()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyVariant();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public BadgeVariant Variant
    {
        get => (BadgeVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Brush ForegroundBrush
    {
        get => (Brush)GetValue(ForegroundBrushProperty);
        set => SetValue(ForegroundBrushProperty, value);
    }

    public Brush BackgroundBrush
    {
        get => (Brush)GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    }

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBadge badge) badge.ApplyVariant();
    }

    private void ApplyVariant()
    {
        // Map the variant to existing brushes from the merged theme. The
        // background is a tinted version of the same hue (BgTertiary acts as
        // a neutral container; the dot + text carry the saturated colour).
        var (fg, _) = Variant switch
        {
            BadgeVariant.Ok => ("OkBrush", "BgTertiaryBrush"),
            BadgeVariant.Warning => ("WarningBrush", "BgTertiaryBrush"),
            BadgeVariant.Danger => ("DangerBrush", "BgTertiaryBrush"),
            BadgeVariant.Info => ("InfoBrush", "BgTertiaryBrush"),
            _ => ("TextSecondaryBrush", "BgTertiaryBrush"),
        };

        if (TryFindResource(fg) is Brush f) ForegroundBrush = f;
        if (TryFindResource("BgTertiaryBrush") is Brush b) BackgroundBrush = b;
    }
}
