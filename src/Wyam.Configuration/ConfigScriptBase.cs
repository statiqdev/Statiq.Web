using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core;
using Wyam.Core.Documents;
using Wyam.Core.Execution;

namespace Wyam.Configuration
{
    /// <summary>
    /// This is the base class used for the generated configuration script. Put any properties or
    /// methods you want the configuration script to have access to in here.
    /// </summary>
    public abstract class ConfigScriptBase
    {
        private readonly Engine _engine;

        protected ConfigScriptBase(Engine engine)
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

        public IDocumentFactory DocumentFactory
        {
            get { return _engine.DocumentFactory; }
            set { _engine.DocumentFactory = value; }
        }

        public void SetCustomDocumentType<T>() where T : CustomDocument, new() => 
            _engine.DocumentFactory = new CustomDocumentFactory<T>(_engine.DocumentFactory);
    }
}
