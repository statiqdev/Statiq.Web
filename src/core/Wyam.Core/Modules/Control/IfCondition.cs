using System.Collections;
using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Control
{
    public class IfCondition : ModuleList
    {
        private ContextConfig _contextConfig;

        private DocumentConfig _documentConfig;

        public ContextConfig ContextConfig
        {
            get => _contextConfig;
            set
            {
                _contextConfig = value;
                _documentConfig = null;
            }
        }

        public DocumentConfig DocumentConfig
        {
            get => _documentConfig;
            set
            {
                _documentConfig = value;
                _contextConfig = null;
            }
        }

        public bool IsFinalElse => _contextConfig == null && _documentConfig == null;

        internal IfCondition(DocumentConfig documentConfig, IModule[] modules)
            : base(modules)
        {
            _documentConfig = documentConfig;
        }

        internal IfCondition(ContextConfig contextConfig, IModule[] modules)
            : base(modules)
        {
            _contextConfig = contextConfig;
        }

        internal IfCondition(IModule[] modules)
            : base(modules)
        {
        }
    }
}
