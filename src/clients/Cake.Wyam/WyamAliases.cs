using System;
using Cake.Core;
using Cake.Core.Annotations;

namespace Cake.Wyam
{
    /// <summary>
    /// <para>Contains functionality related to <see href="https://github.com/Wyamio/Wyam">Wyam</see>.</para>
    /// <para>
    /// In order to use the commands for this alias, include the following in your build.cake file to download and install from NuGet.org, or specify the ToolPath within the WyamSettings class:
    /// <code>
    /// #addin "nuget:?package=Cake.Wyam"
    /// #tool "nuget:?package=Wyam"
    /// </code>
    /// </para>
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
                throw new ArgumentNullException(nameof(context));
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
        ///         OutputPath = Directory("C:/Output")
        ///     });
        /// </code>
        /// </example>
        [CakeMethodAlias]
        public static void Wyam(this ICakeContext context, WyamSettings settings)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            WyamRunner runner = new WyamRunner(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools);
            runner.Run(settings);
        }
    }
}
