namespace PHIL_GUI.Helpers
{
    public static class WellMapper
    {
        public static int ToIndex(this string well)
        {
            char row = char.ToLower(well[0]);
            int col = int.Parse(well.Substring(1));
            return (row - 'a') * 12 + (col - 1);
        }

        public static string FromIndex(this int index)
        {
            char row = (char)('a' + index / 12);
            int col = (index % 12) + 1;
            return $"{row}{col}";
        }
    }
}
