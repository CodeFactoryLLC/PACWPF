using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PAC.WPF
{
    /// <summary>
    /// Base class for user control based presentation.
    /// </summary>
    public abstract class UserControlPresentation:UserControl,IPresentation<UserControl>
    {
        private bool _isDisposed;

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
        public UserControl Presentation => this;

        /// <summary>
        /// Gets the window that hosts this presentation. 
        /// </summary>
        /// <returns>Return back the target window that hosts this presentation, or null if the presentation is not set in a target window.</returns>
        public Window GetHostingWindow()
        {
            return Window.GetWindow(this);
        }

        #endregion
    }
}
