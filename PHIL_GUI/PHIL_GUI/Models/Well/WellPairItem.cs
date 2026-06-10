using CommunityToolkit.Mvvm.ComponentModel;

namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents a pair of wells in an organ-on-chip configuration.
    /// Pairs consist of an inlet well and an outlet well.
    /// </summary>
    public class WellPairItem : ObservableObject
    {
        /// <summary>
        /// Gets or sets the inlet well of the pair.
        /// </summary>
        public WellItem In { get; set; }
        /// <summary>
        /// Gets or sets the outlet well of the pair.
        /// </summary>
        public WellItem Out { get; set; }
        /// <summary>
        /// Gets or sets the pair index (1-based numbering).
        /// </summary>
        public int PairIndex { get; set; }

        private bool isVisible;
        /// <summary>
        /// Gets or sets whether this well pair is visible in the UI.
        /// </summary>
        public bool IsVisible
        {
            get => isVisible;
            set => SetProperty(ref isVisible, value);
        }

        private bool isSelected;
        /// <summary>
        /// Gets or sets whether this well pair is currently selected.
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        /// <summary>
        /// Initializes a new instance of the WellPairItem class.
        /// </summary>
        /// <param name="pairIndex">The pair index.</param>
        /// <param name="pair1">The inlet well.</param>
        /// <param name="pair2">The outlet well.</param>
        /// <param name="isVisible">Whether the pair is visible (default: true).</param>
        public WellPairItem(int pairIndex, WellItem pair1, WellItem pair2, bool isVisible = true)
        {
            In = pair1;
            Out = pair2;
            PairIndex = pairIndex;
            IsVisible = isVisible;
        }
    }
}
