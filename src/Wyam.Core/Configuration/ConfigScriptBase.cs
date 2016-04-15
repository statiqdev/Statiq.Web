using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Core.Documents;

namespace Wyam.Core.Configuration
{
    /// <summary>
    /// This is the base class used for the generated configuration script. Put any properties or
    /// methods you want the configuration script to have access to in here.
    /// </summary>
    public abstract class ConfigScriptBase
    {
        private readonly IEngine _engine;

        protected ConfigScriptBase(IEngine engine)
        {
            _engine = engine;
        }

        public abstract void Run();

        public IPipelineCollection Pipelines => _engine.Pipelines;

        public IFileSystem FileSystem => _engine.FileSystem;

        public IMetadataDictionary InitialMetadata => _engine.InitialMetadata;

        public IMetadataDictionary GlobalMetadata => _engine.GlobalMetadata;

        public ISettings Settings => _engine.Settings;

        public string ApplicationInput => _engine.ApplicationInput;

        public IEngine Engine => _engine;

        public void SetCustomDocumentType<T>() where T : CustomDocument, new()
        {
            Engine.DocumentFactory = new CustomDocumentFactory<T>(Engine.DocumentFactory);
        }
    }
}
