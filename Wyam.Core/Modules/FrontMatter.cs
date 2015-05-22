using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    // This scans the input content for the specified delimiter and then runs the result through the specified modules
    // All resulting metadata is merged with (and takes precedence over) the input document metadata
    // If multiple output documents are generated from one input document, they are all returned as outputs
    // The input content sans the front matter is returned as the content for each output
    public class FrontMatter : IModule
    {
        private readonly string _delimiter;
        private readonly bool _repeated;
        private readonly IModule[] _modules;
        
        public FrontMatter(params IModule[] modules)
        {
            _delimiter = "-";
            _repeated = true;
            _modules = modules;
        }

        public FrontMatter(string delimiter, params IModule[] modules)
        {
            _delimiter = delimiter;
            _repeated = false;
            _modules = modules;
        }

        public FrontMatter(char delimiter, params IModule[] modules)
        {
            _delimiter = new string(delimiter, 1);
            _repeated = true;
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            foreach (IDocument input in inputs)
            {
                List<string> inputLines = input.Content.Split(new[] { '\n' }, StringSplitOptions.None).ToList();
                int delimiterLine = inputLines.FindIndex(x =>
                {
                    string trimmed = x.TrimEnd();
                    return trimmed.Length > 0 && (_repeated ? trimmed.All(y => y == _delimiter[0]) : trimmed == _delimiter);
                });
                if (delimiterLine != -1)
                {
                    string frontMatter = string.Join("\n", inputLines.Take(delimiterLine)) + "\n";
                    inputLines.RemoveRange(0, delimiterLine + 1);
                    string content = string.Join("\n", inputLines);
                    foreach (IDocument result in context.Execute(_modules, new[] { input.Clone(frontMatter) }))
                    {
                        yield return result.Clone(content);
                    }
                }
                else
                {
                    yield return input;
                }
            }
        }
    }
}
