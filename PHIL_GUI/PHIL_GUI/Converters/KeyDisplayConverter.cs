using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Linq;

namespace PHIL_GUI.Converters
{
    /// <summary>
    /// Converts keyboard key names to user-friendly display strings.
    /// Transforms arrow keys to unicode symbols and number keys to simplified formats.
    /// </summary>
    public class KeyDisplayConverter : IValueConverter
    {
        /// <summary>
        /// Singleton instance of the converter for use in XAML bindings.
        /// </summary>
        public static readonly KeyDisplayConverter Instance = new();

        /// <summary>
        /// Converts a keyboard key combination string to a display-friendly format.
        /// </summary>
        /// <param name="value">The key combination string (e.g., "Ctrl+D1" or "Up").</param>
        /// <param name="targetType">The target type of the binding.</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">Culture information (not used).</param>
        /// <returns>A formatted string with arrow symbols and simplified key names (e.g., "Ctrl+1" or "↑").</returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string key) return value!;

            var parts = key.Split('+');
            return string.Join("+", parts.Select(FormatPart));
        }

        /// <summary>
        /// Converts back from display format to original format (not implemented, returns value as-is).
        /// </summary>
        /// <param name="value">The display value.</param>
        /// <param name="targetType">The target type of the binding.</param>
        /// <param name="parameter">Optional parameter (not used).</param>
        /// <param name="culture">Culture information (not used).</param>
        /// <returns>The original value unchanged.</returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value!;

        /// <summary>
        /// Formats a single key part to its display representation.
        /// Converts arrow keys to unicode symbols, D0-D9 to 0-9, and NumPad keys to Num0-Num9.
        /// </summary>
        /// <param name="part">The key part to format.</param>
        /// <returns>The formatted key display string.</returns>
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