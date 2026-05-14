using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace PHIL_GUI.Converters
{
    public class KeyDisplayConverter : IValueConverter
    {
        public static readonly KeyDisplayConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string key) return value!;

            var parts = key.Split('+');
            return string.Join("+", parts.Select(FormatPart));
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value!;

        private static string FormatPart(string part) => part switch
        {
            "Up" => "↑",
            "Down" => "↓",
            "Left" => "←",
            "Right" => "→",

            "D0" => "0",
            "D1" => "1",
            "D2" => "2",
            "D3" => "3",
            "D4" => "4",
            "D5" => "5",
            "D6" => "6",
            "D7" => "7",
            "D8" => "8",
            "D9" => "9",

            "NumPad0" => "Num0",
            "NumPad1" => "Num1",
            "NumPad2" => "Num2",
            "NumPad3" => "Num3",
            "NumPad4" => "Num4",
            "NumPad5" => "Num5",
            "NumPad6" => "Num6",
            "NumPad7" => "Num7",
            "NumPad8" => "Num8",
            "NumPad9" => "Num9",

            _ => part
        };
    }
}