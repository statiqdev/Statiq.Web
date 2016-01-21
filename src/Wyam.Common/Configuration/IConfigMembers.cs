using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// Defines members that should be available within the configuration script.
    /// Any public methods or properties that are declared in the implementing class
    /// will be available from within the configuration script. The loading order
    /// of implementers is undetermined and any conflicts will result in a warning.
    /// </summary>
    public interface IConfigMembers
    {
        /// <summary>
        /// Initializes the implementing class with a <see cref="IConfig"/> that holds
        /// various information about the current configuration.
        /// </summary>
        /// <param name="config">The current configuration.</param>
        /// <returns>
        /// <c>true</c> if the members of the implementing class should be made available
        /// in the configuration script, <c>false</c> if not.
        /// </returns>
        bool Initialize(IConfig config);
    }
}
