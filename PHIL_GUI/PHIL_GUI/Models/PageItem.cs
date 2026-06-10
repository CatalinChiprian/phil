namespace PHIL_GUI.Models
{
    /// <summary>
    /// Represents a navigation page item with title, view model, and optional icon.
    /// Used for navigation menus and tabbed interfaces.
    /// </summary>
    public class PageItem
    {
        /// <summary>
        /// Gets or sets the display title of the page.
        /// </summary>
        public string Title { set; get; }
        /// <summary>
        /// Gets or sets the view model associated with this page.
        /// </summary>
        public object ViewModel { set; get; }
        /// <summary>
        /// Gets or sets the SVG path data for the page icon (optional).
        /// </summary>
        public string IconData { set; get; }

        /// <summary>
        /// Initializes a new instance of the PageItem class.
        /// </summary>
        /// <param name="title">The display title of the page.</param>
        /// <param name="viewModel">The view model for the page.</param>
        /// <param name="iconData">Optional SVG path data for the page icon.</param>
        public PageItem(string title,  object viewModel, string iconData = null)
        {
            Title = title;
            ViewModel = viewModel;
            IconData = iconData;
        }
    }
}
