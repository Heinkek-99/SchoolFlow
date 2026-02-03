using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SchoolFlow.Desktop.Converters;

/// <summary>
/// Convertit un booléen en Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Si le paramètre est "Inverse", on inverse la logique
            if (parameter?.ToString() == "Inverse")
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility visibility && visibility == Visibility.Visible;
    }
}

/// <summary>
/// Convertit un montant en format monnaie FCFA.
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return $"{decimalValue:N0} FCFA";
        }
        if (value is double doubleValue)
        {
            return $"{doubleValue:N0} FCFA";
        }
        if (value is int intValue)
        {
            return $"{intValue:N0} FCFA";
        }
        return "0 FCFA";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string strValue)
        {
            strValue = strValue.Replace("FCFA", "").Replace(" ", "").Replace(",", "");
            if (decimal.TryParse(strValue, out var result))
            {
                return result;
            }
        }
        return 0m;
    }
}

/// <summary>
/// Convertit null en Visibility (Visible si non-null).
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNull = value == null;
        
        if (parameter?.ToString() == "Inverse")
        {
            return isNull ? Visibility.Visible : Visibility.Collapsed;
        }
        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convertit une valeur négative en couleur (rouge pour négatif/impayé, vert pour positif/payé).
/// </summary>
public class SoldeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue <= 0 ? "#27AE60" : "#E74C3C"; // Vert si soldé, Rouge si impayé
        }
        return "#95A5A6"; // Gris par défaut
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convertit une date en format lisible.
/// </summary>
public class DateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateValue)
        {
            string format = parameter?.ToString() ?? "dd/MM/yyyy HH:mm";
            return dateValue.ToString(format);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (DateTime.TryParse(value?.ToString(), out var result))
        {
            return result;
        }
        return DateTime.MinValue;
    }
}
