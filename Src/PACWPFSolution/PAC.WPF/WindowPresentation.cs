using System;
using System.ComponentModel;
using System.Windows;

namespace PAC.WPF
{
    /// <summary>
    /// Base class implementation for a windows based presentation.
    /// </summary>
    public  abstract class WindowPresentation:Window,IWindowPresentation
    {
        /// <summary>
        /// Tracks if the presentation has been disposed.
        /// </summary>
        private bool _isDisposed;


        /// <summary>
        /// Initializes the base implementation of the presentation.
        /// </summary>
        protected WindowPresentation()
        {
            _isDisposed = false;
            this.Closing += WindowPresentation_Closing;

        }

        /// <summary>
        /// Handler for the closing event.
        /// </summary>
        /// <param name="sender">Presentation itself</param>
        /// <param name="e">Cancel event args</param>
        private void WindowPresentation_Closing(object sender, CancelEventArgs e)
        {
            OnPresenterClosing(sender,e);
        }

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            ReleaseResources();
            _isDisposed = true;

        }

        #endregion

        #region Implementation of IReleaseResources

        /// <summary>
        /// Called by the disposal process to release resources that have dependencies in other layers of PAC.
        /// </summary>
        public void ReleaseResources()
        {
            try
            {
                this.Closing -= WindowPresentation_Closing;

                ReleaseContract();
                AutomatedReleaseContract();
                ReleaseInternalResources();

            }
            catch (Exception unhandledError)
            {
                //Add exception management
            }

        }

        /// <summary>
        /// Releases contract resources that are subscribed to by the hosting controller.
        /// </summary>
        protected abstract void ReleaseContract();

        /// <summary>
        /// Releases contract resources that are subscribed to by the hosting controller. Created by code automation.
        /// </summary>
        protected virtual void AutomatedReleaseContract()
        {
            //Intentionally blank.
        }

        /// <summary>
        /// Release logic to be applied to any objects that require special handling internal this classes implementation.
        /// </summary>
        protected abstract void ReleaseInternalResources();

        /// <summary>
        /// Flag that determined if this controller has been disposed.
        /// </summary>
        public bool IsDisposed => _isDisposed;

        #endregion

        #region Implementation of IPresentation<out Window>

        /// <summary>
        /// The control or window that is managed by this presentation.
        /// </summary>
        public Window Presentation => this;

        /// <summary>
        /// Gets the window that hosts this presentation. 
        /// </summary>
        /// <returns>Return back the target window that hosts this presentation, or null if the presentation is not set in a target window.</returns>
        public Window GetHostingWindow()
        {
            return this;
        }

        #endregion

        #region Implementation of IWindowPresentation

        /// <summary>
        /// Displays a window as a dialog and the start location of the window.
        /// </summary>
        /// <param name="location">The location the dialog will start up as.</param>
        /// <param name="owner">The owner of the window this is optional unless you set to center on owner.</param>
        public void ShowPresentationDialog(WindowStartupLocation location, Window owner = null)
        {
            this.WindowStartupLocation = location;
            if (owner != null) this.Owner = owner;
            this.ShowDialog();
            this.Owner = null;
        }

        /// <summary>
        /// Displays a window and the start location of the window will be placed in.
        /// </summary>
        /// <param name="location">The location the dialog will start up as.</param>
        /// <param name="owner">The owner of the window this is optional unless you set to center on owner.</param>
        public void ShowPresentation(WindowStartupLocation location, Window owner = null)
        {
            this.WindowStartupLocation = location;
            if (owner != null) this.Owner = owner;
            this.Show();
        }

        /// <summary>
        /// Event that notifies that the presenter is closing.
        /// </summary>
        public event CancelEventHandler PresenterClosing;

        /// <summary>
        /// Used to raise the event Presenter Closing
        /// </summary>
        /// <param name="owner">Hosting object that is closing</param>
        /// <param name="eventArgs">The cancel event args </param>
        protected virtual void OnPresenterClosing(object owner, CancelEventArgs eventArgs)
        {
            var presenterClosing = this.PresenterClosing;

            presenterClosing?.Invoke(owner,eventArgs);
        }

        #endregion
    }
}
