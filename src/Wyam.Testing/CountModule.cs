using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Testing
{
    public class CountModule : IModule
    {
        public int AdditionalOutputs { get; set; }  // Controls how many additional outputs are spawned
        public string ValueKey { get; set; }  // This is the key used for storing the value in the metadata
        public int Value { get; set; }  // This is incremented on every call and output and added to the metadata
        public int ExecuteCount { get; set; }
        public int InputCount { get; set; }
        public int OutputCount { get; set; }
        public bool CloneSource { get; set; }  // Indicates whether the clone call should output a source

        public CountModule(string valueKey)
        {
            ValueKey = valueKey;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            int sourceCount = 0;
            ExecuteCount++;
            foreach (IDocument input in inputs)
            {
                InputCount++;
                for (int c = 0; c < AdditionalOutputs + 1; c++)
                {
                    OutputCount++;
                    Value++;
                    if(CloneSource)
                    {
                        yield return context.GetDocument(input, new FilePath(ValueKey + sourceCount++, true), input.Content == null ? Value.ToString() : input.Content + Value, 
                            new Dictionary<string, object> { { ValueKey, Value } });
                    }
                    else
                    {
                        yield return context.GetDocument(input, input.Content == null ? Value.ToString() : input.Content + Value,
                            new Dictionary<string, object> { { ValueKey, Value } });
                    }
                }
            }
        }
    }
}
