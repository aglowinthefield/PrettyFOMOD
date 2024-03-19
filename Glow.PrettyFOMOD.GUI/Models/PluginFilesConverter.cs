using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Glow.PrettyFOMOD.GUI.Models;

public class PluginFilesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var fileLists = value as Collection<FileList> ?? [];
        return fileLists.SelectMany(f => f.File);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}