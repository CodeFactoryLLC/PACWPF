using System;

namespace PAC
{
    /// <summary>
    /// Contract that makes standard release logic available to all PAC contracts.
    /// </summary>
    public interface IReleaseResources:IDisposable
    {
        /// <summary>
        /// Called by the disposal process to release resources that have dependencies in other layers of PAC.
        /// </summary>
        void ReleaseResources();

        /// <summary>
        /// Flag that determined if this controller has been disposed.
        /// </summary>
        bool IsDisposed { get; }
    }

}
