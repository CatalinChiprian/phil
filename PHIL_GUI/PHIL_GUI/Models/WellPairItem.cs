namespace PHIL_GUI.Models
{
    public class WellPairItem
    {
        public WellItem In { get; set; }
        public WellItem Out { get; set; }
        public int PairIndex { get; set; }

        public WellPairItem(int pairIndex, WellItem pair1, WellItem pair2)
        {
            In = pair1;
            Out = pair2;
            PairIndex = pairIndex;
        }
    }
}
