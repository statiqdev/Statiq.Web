using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Modules.TextGeneration
{
    public class GenerateContent : RantModule
    {
        public GenerateContent(object template) : base(template)
        {
        }

        public GenerateContent(ContextConfig template) : base(template)
        {
        }

        public GenerateContent(DocumentConfig template) : base(template)
        {
        }

        public GenerateContent(params IModule[] modules) : base(modules)
        {
        }

        protected override IDocument Execute(string content, IDocument input)
        {
            return input.Clone(content);
        }
    }
}
