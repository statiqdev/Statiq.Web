using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// Implement this interface for any module you want to support the <c>AsNewDocuments()</c>
    /// extension method, which overrides document creation within the module to always produce
    /// new documents instead of cloning existing ones.
    /// </summary>
    public interface IAsNewDocuments
    {
    }
}
