namespace PHIL_GUI.Models
{
    public interface IWellPlateItem : IWellPlateItemBase
    {
        WellItem SelectWell(string name);
    }
}
