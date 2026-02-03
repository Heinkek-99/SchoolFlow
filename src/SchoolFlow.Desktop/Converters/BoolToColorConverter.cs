using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SchoolFlow.Desktop.Converters;

/// <summary>
/// Convertit un booléen en couleur (vert si true, rouge si false).
/// Utilisé pour la ventilation des paiements.
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "#27AE60" : "#E74C3C"; // Vert si valide, Rouge sinon
        }
        return "#95A5A6"; // Gris par défaut
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
