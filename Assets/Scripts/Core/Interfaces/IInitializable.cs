using System.Collections;

namespace ElevelLabs.VRAvatar.Core.Interfaces
{
    /// <summary>
    /// Interface for components that require initialization.
    /// Provides a standardized way to initialize components in sequence.
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Initializes the component and returns a coroutine for sequential initialization.
        /// </summary>
        /// <returns>Coroutine for sequential initialization.</returns>
        IEnumerator Initialize();
        
        /// <summary>
        /// Gets whether the component has been initialized.
        /// </summary>
        bool IsInitialized { get; }
    }
}