using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace DiagCore.App.Controls;

/// <summary>
/// Hero-card tile for dashboard-style displays. Shows an icon + caption
/// header, a large numeric value, an optional sub-caption and an optional
/// percentage bar. The accent brush automatically follows the supplied
/// <see cref="Percent"/> via three threshold breakpoints (green &lt; 60,
/// amber &lt; 85, red otherwise).
/// </summary>
public partial class StatTile : UserControl
{
    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        nameof(Caption), typeof(string), typeof(StatTile), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(StatTile), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SubCaptionProperty = DependencyProperty.Register(
        nameof(SubCaption), typeof(string), typeof(StatTile), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(SymbolRegular), typeof(StatTile),
        new PropertyMetadata(SymbolRegular.Info24));

    public static readonly DependencyProperty PercentProperty = DependencyProperty.Register(
        nameof(Percent), typeof(double), typeof(StatTile),
        new PropertyMetadata(0d, OnPercentChanged));

    public static readonly DependencyProperty ShowBarProperty = DependencyProperty.Register(
        nameof(ShowBar), typeof(bool), typeof(StatTile),
        new PropertyMetadata(false));

    public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
        nameof(AccentBrush), typeof(Brush), typeof(StatTile),
        new PropertyMetadata(default(Brush)));

    public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register(
        nameof(BarWidth), typeof(double), typeof(StatTile),
        new PropertyMetadata(0d));

    public StatTile()
    {
        InitializeComponent();
        SizeChanged += (_, _) => UpdateBarWidth();
        Loaded += (_, _) =>
        {
            ApplyAccentForPercent(Percent);
            UpdateBarWidth();
        };
    }

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string SubCaption
    {
        get => (string)GetValue(SubCaptionProperty);
        set => SetValue(SubCaptionProperty, value);
    }

    public SymbolRegular Icon
    {
        get => (SymbolRegular)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Percentage 0-100. Drives the optional bar and the accent colour.</summary>
    public double Percent
    {
        get => (double)GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }

    public bool ShowBar
    {
        get => (bool)GetValue(ShowBarProperty);
        set => SetValue(ShowBarProperty, value);
    }

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public double BarWidth
    {
        get => (double)GetValue(BarWidthProperty);
        set => SetValue(BarWidthProperty, value);
    }

    private static void OnPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatTile tile)
        {
            tile.ApplyAccentForPercent((double)e.NewValue);
            tile.UpdateBarWidth();
        }
    }

    private void ApplyAccentForPercent(double percent)
    {
        // Lazily resolve the brushes from the merged app dictionary so a theme
        // change at runtime keeps the colours in sync.
        var resource = percent switch
        {
            < 60 => "OkBrush",
            < 85 => "WarningBrush",
            _ => "DangerBrush",
        };

        if (TryFindResource(resource) is Brush brush)
        {
            AccentBrush = brush;
        }
    }

    private void UpdateBarWidth()
    {
        if (!ShowBar || ActualWidth <= 0) return;
        var pct = Math.Clamp(Percent, 0, 100);
        // Card has 16px padding on each side; the bar host stretches across the
        // remaining width.
        var available = Math.Max(0, ActualWidth - 32);
        BarWidth = available * (pct / 100d);
    }
}
