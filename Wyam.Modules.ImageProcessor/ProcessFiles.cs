using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Abstractions;

namespace Wyam.Modules.ImageProcessor
{
    public class ProcessFiles : IModule
    {
        private readonly Func<IDocument, string> _sourcePath;
        private Func<string, string> _destinationPath;
        private SearchOption _searchOption = System.IO.SearchOption.AllDirectories;
        private Func<string, bool> _where = null;

        public ProcessFiles(Func<IDocument, string> sourcePath)
        {
            if (sourcePath == null)
            {
                throw new ArgumentNullException("sourcePath");
            }

            _sourcePath = sourcePath;
        }

        public ProcessFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }

            _sourcePath = m => searchPattern;
        }

        public ProcessFiles SearchOption(SearchOption searchOption)
        {
            _searchOption = searchOption;
            return this;
        }

        public ProcessFiles AllDirectories()
        {
            _searchOption = System.IO.SearchOption.AllDirectories;
            return this;
        }

        public ProcessFiles TopDirectoryOnly()
        {
            _searchOption = System.IO.SearchOption.TopDirectoryOnly;
            return this;
        }

        public ProcessFiles Where(Func<string, bool> predicate)
        {
            _where = predicate;
            return this;
        }

        public ProcessFiles To(Func<string, string> destinationPath)
        {
            if (destinationPath == null)
            {
                throw new ArgumentNullException("destinationPath");
            }

            _destinationPath = destinationPath;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var documents = from input in inputs
                            let p = _sourcePath(input)
                            where p != null
                            let path = Path.Combine(context.InputFolder, p)
                            let fileRoot = Path.GetDirectoryName(path)
                            where fileRoot != null && Directory.Exists(fileRoot)
                            select new
                            {
                                Input = input,
                                Listing = Directory.EnumerateFiles(fileRoot, Path.GetFileName(path), _searchOption).Where(x => _where == null || _where(x))
                            } into g
                            from m in g.Listing
                            let ext = Path.GetExtension(m)
                            let binary = File.ReadAllBytes(m)
                            select g.Input.Clone(Convert.ToBase64String(binary), new Dictionary<string, object>
                            {
                                [MetadataKeys.SourceFilePath] = m,
                                [MetadataKeys.SourceFileExt] = ext,
                                [MetadataKeys.Base64] = true
                            });

            return documents;
        }
    }
}
