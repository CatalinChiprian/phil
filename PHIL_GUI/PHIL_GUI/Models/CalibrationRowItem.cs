namespace PHIL_GUI.Models
{
    public class CalibrationRowItem
    {
        public string Well { get; set; }
        public string XY { get; set; }
        public float ErrL { get; set; }
        public float ErrR { get; set; }

        public CalibrationRowItem(string well, string xy, float errL, float errR)
        {
            Well = well;
            XY = xy;
            ErrL = errL;
            ErrR = errR;
        }
    }
}
