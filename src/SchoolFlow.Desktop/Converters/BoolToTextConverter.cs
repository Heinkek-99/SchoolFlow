using System.Globalization;
using System.Windows.Data;

namespace SchoolFlow.Desktop.Converters;

/// <summary>
/// Convertit un booléen en texte (vrai|faux).
/// Usage: ConverterParameter="TexteVrai|TexteFaux"
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string texts)
        {
            var parts = texts.Split('|');
            if (parts.Length == 2)
            {
                return boolValue ? parts[0] : parts[1];
            }
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
