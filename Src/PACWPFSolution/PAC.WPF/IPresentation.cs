using System.Windows;
using System.Windows.Threading;

namespace PAC.WPF
{
    /// <summary>
    /// Contract implemented by all presentations
    /// </summary>
    public interface IPresentation<out T>:IReleaseResources where T:FrameworkElement
    {

        /// <summary>
        /// The control or window that is managed by this presentation.
        /// </summary>
        T Presentation { get; }

        /// <summary>
        /// The dispatcher that supports this presentation.
        /// </summary>
        Dispatcher Dispatcher { get; }

        /// <summary>
        /// Gets the window that hosts this presentation. 
        /// </summary>
        /// <returns>Return back the target window that hosts this presentation, or null if the presentation is not set in a target window.</returns>
        Window GetHostingWindow();

    }
}
