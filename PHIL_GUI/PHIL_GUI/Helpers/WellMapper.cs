using PHIL_GUI.Models;
using System.Collections.Generic;
using System.Linq;

namespace PHIL_GUI.Helpers
{
    public static class WellMapper
    {
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

        public static IEnumerable<int> ToIndices(this IEnumerable<string> wells)
        {
            return wells.Select(w => w.ToIndex());
        }

        public static string FromIndex(this int index)
        {
            char row = (char)('A' + index / WellPlateItemBase.COLUMN_COUNT);
            int col = (index % WellPlateItemBase.COLUMN_COUNT) + 1;
            return $"{row}{col}";
        }

        public static int ChannelToInWellIndex(int channel)
        {
            int zeroBased = channel - 1;

            int oocRow = zeroBased / WellPlateItemBase.ROW_COUNT;
            int oocCol = zeroBased % WellPlateItemBase.ROW_COUNT;

            int wellRow = oocRow * 2;
            int wellCol = oocCol * 2;

            return wellRow * 12 + wellCol;
        }

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
