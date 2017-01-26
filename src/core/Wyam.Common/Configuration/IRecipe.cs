using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// A recipe configures a <see cref="IEngine"/> in a predefined way using code.
    /// Recipes should have an empty default constructor and shouldn't maintain
    /// any state.
    /// </summary>
    public interface IRecipe
    {
        /// <summary>
        /// Applies the recipe to the engine.
        /// </summary>
        /// <param name="engine">The engine.</param>
        void Apply(IEngine engine);

        /// <summary>
        /// Scaffolds an example for the recipe in the specified directory.
        /// </summary>
        /// <param name="configFile">
        /// The configuration file or <c>null</c> if the user 
        /// chose not to overwrite an existing configuration file. An existing configuration file
        /// will not be automatically deleted, nor will a new configuration file be automatically
        /// created. If new configuration file content needs to be written,
        /// it's up to the recipe to create it or delete the existing file.
        /// </param>
        /// <param name="inputDirectory">The directory to place the example in.</param>
        void Scaffold(IFile configFile, IDirectory inputDirectory);
    }
}
