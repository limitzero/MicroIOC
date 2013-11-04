namespace MicroIOC.Core
{
    /// <summary>
    /// Enumeration configuring the lifecycle of the dependency in the container.
    /// </summary>
    public enum ComponentLifeCycles
    {
        /// <summary>
        /// Components created on an as needed basis from the container.
        /// </summary>
        Transient, 

        /// <summary>
        /// Components created once, and the instance is re-used on each request for resolving the component.
        /// </summary>
        Singleton
    }
}