namespace MicroIOC.Core
{
    /// <summary>
    /// Contract for segmenting out component installations into manageable pieces for the container.
    /// </summary>
    public interface IInstaller
    {
        /// <summary>
        /// This will configure the components in the container and their dependencies.
        /// </summary>
        /// <param name="container">Current<seealso cref="IContainer">container</seealso>used to configure the components</param>
        void Configure(IContainer container);
    }
}