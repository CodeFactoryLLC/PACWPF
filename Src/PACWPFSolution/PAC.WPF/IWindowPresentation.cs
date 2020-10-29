using System.ComponentModel;
using System.Windows;

namespace PAC.WPF
{
    /// <summary>
    /// Contract to be implemented by windows based presentations.
    /// </summary>
    public interface IWindowPresentation:IPresentation<Window>
    {
        /// <summary>
        /// Displays a window as a dialog and the start location of the window.
        /// </summary>
        /// <param name="location">The location the dialog will start up as.</param>
        /// <param name="owner">The owner of the window this is optional unless you set to center on owner.</param>
        void ShowPresentationDialog(WindowStartupLocation location, Window owner = null);

        /// <summary>
        /// Displays a window and the start location of the window will be placed in.
        /// </summary>
        /// <param name="location">The location the dialog will start up as.</param>
        /// <param name="owner">The owner of the window this is optional unless you set to center on owner.</param>
        void ShowPresentation(WindowStartupLocation location, Window owner = null);

        /// <summary>
        /// Event that notifies that the presenter is closing.
        /// </summary>
        event CancelEventHandler PresenterClosing;

    }
}