using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DiagCore.App.Controls;

/// <summary>
/// "Label: Value" row used throughout the diagnostic views. Two columns
/// (fixed-width label, flexible value) with optional monospace +
/// status colour for the value. Label uses TextSecondaryBrush, value
/// defaults to TextPrimaryBrush.
/// </summary>
public partial class KeyValueRow : UserControl
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(KeyValueRow), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(string), typeof(KeyValueRow), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueFontFamilyProperty = DependencyProperty.Register(
        nameof(ValueFontFamily), typeof(FontFamily), typeof(KeyValueRow), new PropertyMetadata(default(FontFamily)));

    public static readonly DependencyProperty ValueForegroundProperty = DependencyProperty.Register(
        nameof(ValueForeground), typeof(Brush), typeof(KeyValueRow), new PropertyMetadata(default(Brush)));

    public KeyValueRow()
    {
        InitializeComponent();
        // Pick up TextPrimaryBrush from the merged theme as the default value
        // colour. This keeps the resource live (theme changes propagate).
        Loaded += (_, _) =>
        {
            if (ValueForeground is null && TryFindResource("TextPrimaryBrush") is Brush b)
            {
                ValueForeground = b;
            }
        };
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public FontFamily ValueFontFamily
    {
        get => (FontFamily)GetValue(ValueFontFamilyProperty);
        set => SetValue(ValueFontFamilyProperty, value);
    }

    public Brush ValueForeground
    {
        get => (Brush)GetValue(ValueForegroundProperty);
        set => SetValue(ValueForegroundProperty, value);
    }
}
