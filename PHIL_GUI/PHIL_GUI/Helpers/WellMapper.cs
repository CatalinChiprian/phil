using PHIL_GUI.Models;
using System.Collections.Generic;
using System.Linq;

namespace PHIL_GUI.Helpers
{
    /// <summary>
    /// Provides static extension methods for converting between well names, indices, and bitmasks.
    /// Supports both standard well plate notation (e.g., "A1") and organ-on-chip channel indices.
    /// </summary>
    public static class WellMapper
    {
        /// <summary>
        /// Converts a well name or channel number to a zero-based well index.
        /// </summary>
        /// <param name="well">Well name (e.g., "A1", "B12") or channel number as a string.</param>
        /// <returns>Zero-based index in the well plate array.</returns>
        public static int ToIndex(this string well)
        {
            // CASE 1: standard well
            if (char.IsLetter(well[0]))
            {
                char row = char.ToLower(well[0]);
                int col = int.Parse(well.Substring(1));
                return (row - 'a') * WellPlateItemBase.COLUMN_COUNT + (col - 1);
            }

            // CASE 2: channel
            int channelIndex = int.Parse(well);

            return ChannelToInWellIndex(channelIndex);
        }

        /// <summary>
        /// Converts a collection of well names to their corresponding zero-based indices.
        /// </summary>
        /// <param name="wells">Collection of well names or channel numbers.</param>
        /// <returns>Enumerable of zero-based well indices.</returns>
        public static IEnumerable<int> ToIndices(this IEnumerable<string> wells)
        {
            return wells.Select(w => w.ToIndex());
        }

        /// <summary>
        /// Converts a zero-based well index to its standard well plate notation.
        /// </summary>
        /// <param name="index">Zero-based well index.</param>
        /// <returns>Well name in standard notation (e.g., "A1", "H12").</returns>
        public static string FromIndex(this int index)
        {
            char row = (char)('A' + index / WellPlateItemBase.COLUMN_COUNT);
            int col = (index % WellPlateItemBase.COLUMN_COUNT) + 1;
            return $"{row}{col}";
        }

        /// <summary>
        /// Converts an organ-on-chip channel number to its corresponding well index.
        /// Maps channel positions to the appropriate well locations in a 96-well plate layout.
        /// </summary>
        /// <param name="channel">Channel number (1-based).</param>
        /// <returns>Zero-based well index corresponding to the channel position.</returns>
        public static int ChannelToInWellIndex(int channel)
        {
            int zeroBased = channel - 1;

            int oocRow = zeroBased / WellPlateItemBase.PAIRS_PER_QUADRANT;
            int oocCol = zeroBased % WellPlateItemBase.PAIRS_PER_QUADRANT;

            int wellRow = oocRow * 2;
            int wellCol = oocCol * 2;

            return wellRow * 12 + wellCol;
        }

        /// <summary>
        /// Converts a collection of well indices into a 12-byte bitmask representation.
        /// Each bit represents one well in a 96-well plate (96 bits = 12 bytes).
        /// </summary>
        /// <param name="wells">Collection of zero-based well indices.</param>
        /// <returns>12-byte array with bits set for each well index.</returns>
        public static byte[] WellIndicesToBitmask(this IEnumerable<int> wells)
        {
            // 96 Wells bits bitmask -> 8 bits per bytes -> 12 bytes array
            var bitmask = new byte[12];

            foreach (var index in wells)
            {
                if (index < 0 || index >= 96)
                    continue;

                int byteIndex = index / 8;
                int bitIndex = index % 8;

                bitmask[byteIndex] |= (byte)(1 << bitIndex);
            }

            return bitmask;
        }
    }
}
