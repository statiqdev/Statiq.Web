using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Tests
{
    public class CountModule : Module
    {
        public int AdditionalOutputs { get; set; }  // Controls how many additional outputs are spawned
        public int PrepareCount { get; set; }
        public string ValueKey { get; set; }  // This is the key used for storing the value in the metadata
        public int Value { get; set; }  // This is incremented on every call and output and added to the metadata
        public int OutputCount { get; set; }
        public int ExecuteCount { get; set; }

        public CountModule(string valueKey)
        {
            ValueKey = valueKey;
        }
        
        protected internal override IEnumerable<IModuleContext> Prepare(IModuleContext context)
        {
            PrepareCount++;
            for(int c = 0 ; c < AdditionalOutputs + 1 ; c++)
            {
                Value++;
                OutputCount++;
                yield return context.Clone(Value, new Dictionary<string, object> {{ValueKey, Value}});
            }
        }

        protected internal override string Execute(IModuleContext context, string content)
        {
            ExecuteCount++;
            Value++;
            return content == null ? Value.ToString() : content + Value;
        }
    }
}
