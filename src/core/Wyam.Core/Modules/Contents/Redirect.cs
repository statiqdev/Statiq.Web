using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Handles redirected content by creating pages with meta refresh tags or other redirect files.
    /// </summary>
    /// <remarks>
    /// <para>When content moves you need some way to redirect from the old location to the new location.
    /// This is especially true when moving content from one publishing system to another that might
    /// have different conventions for things like paths.</para>
    /// <para>This module lets you manage redirected content
    /// by generating special pages that contain a "meta refresh tag". This tag tells client browsers
    /// that the content has moved and to redirect to the new location. Google and other search engines
    /// also understand meta refresh tags and will adjust their search indexes accordingly.</para>
    /// <para>Alternatively (or additionally), you can also create host-specific redirect files to
    /// control redirection on the server.</para>
    /// <para>By default, this module will read the paths that need to be redirected from the
    /// <c>RedirectFrom</c> metadata key. One or more paths can be specified in this metadata and
    /// corresponding redirect files will be created for each.</para>
    /// <para>This module outputs any meta refresh pages as well as any additional redirect files
    /// you specify. It does not output the original input files.</para>
    /// </remarks>
    /// <metadata name="RelativeFilePath" type="FilePath">Relative path to the output meta refresh 
    /// page or output redirect listing file.</metadata>
    /// <metadata name="WritePath" type="FilePath">Relative path to the meta refresh page or 
    /// output redirect listing file (primarily to support the WriteFiles module).</metadata>
    /// <category>Content</category>
    public class Redirect : IModule
    {
        private readonly Dictionary<FilePath, Func<IDictionary<FilePath, string>, string>> _additionalOutputs =
            new Dictionary<FilePath, Func<IDictionary<FilePath, string>, string>>();

        private DocumentConfig _paths = (doc, ctx) => doc.List<FilePath>(Keys.RedirectFrom);
        private bool _metaRefreshPages = true;
        private bool _includeHost = false;
        
        /// <summary>
        /// Controls where the redirected paths come from. By default, values from the metadata
        /// key <c>RedirectFrom</c> are used.
        /// </summary>
        /// <param name="paths">A delegate that should return one or more <see cref="FilePath"/>.</param>
        public Redirect WithPaths(DocumentConfig paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }
            _paths = paths;
            return this;
        }

        /// <summary>
        /// Controls whether meta refresh pages are output.
        /// </summary>
        /// <param name="metaRefreshPages">If <c>true</c>, meta refresh pages are generated.</param>
        public Redirect WithMetaRefreshPages(bool metaRefreshPages = true)
        {
            _metaRefreshPages = metaRefreshPages;
            return this;
        }

        /// <summary>
        /// Indicates whether the host should be automatically included in generated redirect links.
        /// </summary>
        /// <param name="includeHost"><c>true</c> to include the host.</param>
        public Redirect IncludeHost(bool includeHost = true)
        {
            _includeHost = includeHost;
            return this;
        }

        /// <summary>
        /// Adds additional output files that you specify by supplying a delegate that takes a dictionary
        /// of redirected paths to destination URLs.
        /// </summary>
        /// <param name="path">The path of the output file (must be relative).</param>
        /// <param name="content">A delegate that takes a dictionary with keys equal to each redirected file
        /// and values equal to the destination URL. The delegate should return the content of the output file.</param>
        public Redirect WithAdditionalOutput(FilePath path, Func<IDictionary<FilePath, string>, string> content)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("The output path must be relative");
            }
            _additionalOutputs.Add(path, content);
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Iterate redirects and generate all of the per-redirect documents (I.e., meta refresh pages)
            ConcurrentDictionary<FilePath, string> redirects = new ConcurrentDictionary<FilePath, string>();
            List<IDocument> outputs = inputs
                .AsParallel()
                .SelectMany(input =>
                {
                    IReadOnlyList<FilePath> paths = _paths.Invoke<IReadOnlyList<FilePath>>(input, context);
                    if (paths != null)
                    {
                        List<IDocument> metaRefreshDocuments = new List<IDocument>();
                        foreach (FilePath fromPath in paths.Where(x => x != null))
                        {
                            // Make sure it's a relative path
                            if (!fromPath.IsRelative)
                            {
                                Trace.Warning($"The redirect path must be relative for document {input.Source.ToString()}: {fromPath}");
                                continue;
                            }

                            // Record the redirect for additional processing
                            string url = context.GetLink(input, _includeHost);
                            redirects.TryAdd(fromPath, url);
                            
                            // Meta refresh documents
                            FilePath outputPath = fromPath.ChangeExtension(".html");
                            if (_metaRefreshPages)
                            {
                                metaRefreshDocuments.Add(context.GetDocument($@"
<!doctype html>
<html>    
  <head>      
    <title>Redirected</title>      
    <meta http-equiv=""refresh"" content=""0;url='{url}'"" />    
  </head>    
  <body> 
    <p>This page has moved to a <a href=""{url}"">{url}</a></p> 
  </body>  
</html>
                                ", new MetadataItems
                                {
                                    { Keys.RelativeFilePath, outputPath },
                                    { Keys.WritePath, outputPath }
                                }));
                            }
                        }
                        return (IEnumerable<IDocument>)metaRefreshDocuments;
                    }
                    return new IDocument[] {};
                })
                .ToList();  // Need to materialize the parallel operation before creating the additional outputs

            // Generate other output documents if requested
            if(redirects.Count > 0)
            {
                foreach (KeyValuePair<FilePath, Func<IDictionary<FilePath, string>, string>> additionalOutput in _additionalOutputs)
                {
                    string content = additionalOutput.Value(redirects);
                    if (!string.IsNullOrEmpty(content))
                    {
                        outputs.Add(context.GetDocument(content, new MetadataItems
                        {
                            { Keys.RelativeFilePath, additionalOutput.Key },
                            { Keys.WritePath, additionalOutput.Key }
                        }));
                    }
                }
            }

            return outputs;
        }
    }
}
