using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Defines the available plate types for the robot system.
    /// </summary>
    public enum PlateType
    {
        /// <summary>Organ-on-chip microfluidic plate with paired inlet/outlet wells.</summary>
        OrganOnChip,
        /// <summary>Standard 96-well plate (8 rows × 12 columns).</summary>
        Well96
    };
    /// <summary>
    /// Represents the application's settings and configuration.
    /// Manages plate type selection, action recording state, and key bindings.
    /// </summary>
    public class AppSettings : ObservableObject, IPlateContext, IRecordContext
    {
        private PlateType selectedPlateType;
        /// <summary>
        /// Gets or sets the currently selected plate type.
        /// </summary>
        public PlateType SelectedPlateType
        {
            get => selectedPlateType;
            set
            {
                SetProperty(ref selectedPlateType, value);
                OnPropertyChanged(nameof(Is96Well));
            }
        }

        /// <summary>
        /// Gets whether the currently selected plate type is a 96-well plate.
        /// </summary>
        public bool Is96Well => SelectedPlateType == PlateType.Well96;

        private bool areActionRecorded;
        /// <summary>
        /// Gets or sets whether actions will be recorded.
        /// </summary>
        public bool AreActionRecorded
        {
            get => areActionRecorded;
            set => SetProperty(ref areActionRecorded, value);
        }

        /// <summary>
        /// Gets the application's keyboard key bindings configuration.
        /// </summary>
        public AppKeyBindings AppKeyBindings { get; } = new AppKeyBindings();

        /// <summary>
        /// Initializes a new instance of the AppSettings class.
        /// </summary>
        public AppSettings()
        { }
    }
}
