using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    /// <summary>
    /// Evaluates the specified modules with each input document as the initial 
    /// document and then outputs the original input documents without modification.
    /// </summary>
    /// <remarks>
    /// This allows a sequence of modules to execute without impacting the "main" module sequence. 
    /// In other words, Branch executes it's child modules as if there were no Branch module 
    /// in the sequence, but then when it's child modules are done, the main sequence of 
    /// modules is executed as if there were no Branch.
    /// </remarks>
    /// <example>
    /// Assume you have a module, AddOne, that just adds 1 to whatever numeric value is in 
    /// the content of the input document(s). The input and output content of the 
    /// following pipeline should demonstrate what Branch does:
    /// <code>
    ///                     // Input Content      // Output Content
    /// Pipelines.Add(
    ///     AddOne(),       // [Empty]            // 0
    ///     AddOne(),       // 0                  // 1
    ///     AddOne(),       // 1                  // 2
    ///     Branch(
    ///         AddOne(),   // 2                  // 3
    ///         AddOne()    // 3                  // 4
    ///     ),
    ///     AddOne(),       // 2                  // 3
    ///     AddOne()        // 3                  // 4
    /// );
    /// </code>
    /// You can see that the input content to the AddOne modules after the Branch is the 
    /// same as the input content to the AddOne modules inside the branch. The result of 
    /// the modules in the Branch had no impact on those modules that run after the Branch. 
    /// This is true for both content and metadata. If any modules inside the Branch created 
    /// or changed metadata, it would be forgotten once the Branch was done.
    /// </example>
    /// <category>Control</category>
    public class Branch : IModule
    {
        private readonly IModule[] _modules;
        private Func<IDocument, IExecutionContext, bool> _predicate;

        /// <summary>
        /// Evaluates the specified modules with each input document as the initial 
        /// document and then outputs the original input documents without modification.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Branch(params IModule[] modules)
        {
            _modules = modules;
        }
        
        /// <summary>
        /// Limits the documents passed to the child modules to those that satisfy the 
        /// supplied predicate. All original input documents are output without 
        /// modification regardless of whether they satisfy the predicate.
        /// </summary>
        /// <param name="predicate">A delegate that should return a <c>bool</c>.</param>
        public Branch Where(DocumentConfig predicate)
        {
            Func<IDocument, IExecutionContext, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null 
                ? (Func<IDocument, IExecutionContext, bool>)(predicate.Invoke<bool>) 
                : ((x, c) => currentPredicate(x, c) && predicate.Invoke<bool>(x, c));
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> documents = _predicate == null ? inputs : inputs.Where(x => _predicate(x, context));
            context.Execute(_modules, documents);
            return inputs;
        }
    }
}
