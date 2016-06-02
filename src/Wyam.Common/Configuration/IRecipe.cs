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
        /// <param name="directory">The directory to place the example in.</param>
        void Scaffold(IDirectory directory);
    }
}
