using System;
using Cake.Core;
using Cake.Core.Annotations;

namespace Cake.Wyam
{
    /// <summary>
    /// Contains functionality for working with Wyam.
    /// </summary>
    [CakeAliasCategory("Wyam")]
    public static class WyamAliases
    {
        /// <summary>
        /// Runs Wyam using the specified settings.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <example>
        /// <code>
        ///     Wyam();
        /// </code>
        /// </example>
        [CakeMethodAlias]
        public static void Wyam(this ICakeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            Wyam(context, new WyamSettings());
        }

        /// <summary>
        /// Runs Wyam using the specified settings.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="settings">The settings.</param>
        /// <example>
        /// <code>
        ///     Wyam(new WyamSettings()
        ///     {
        ///         OutputDirectory = Directory("C:/Output")
        ///     });
        /// </code>
        /// </example>
        [CakeMethodAlias]
        public static void Wyam(this ICakeContext context, WyamSettings settings)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var runner = new WyamRunner(context.FileSystem, context.Environment, context.ProcessRunner, context.Globber);
            runner.Run(settings);
        }
    }
}