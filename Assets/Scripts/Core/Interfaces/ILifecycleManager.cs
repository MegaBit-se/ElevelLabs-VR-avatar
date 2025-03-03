using System.Collections;

namespace ElevelLabs.VRAvatar.Core.Interfaces
{
    /// <summary>
    /// Interface for components that need to manage application lifecycle events.
    /// Provides methods for handling pause, resume, and shutdown.
    /// </summary>
    public interface ILifecycleManager : IInitializable
    {
        /// <summary>
        /// Pauses the component's operations.
        /// Called when the application is paused.
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resumes the component's operations.
        /// Called when the application resumes from a paused state.
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Shuts down the component and cleans up resources.
        /// Called when the application is shutting down.
        /// </summary>
        void Shutdown();
    }
}