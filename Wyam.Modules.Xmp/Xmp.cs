using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using MetadataExtractor.Formats.Xmp;
using XmpCore;
using System.Globalization;

namespace Wyam.Modules.Xmp
{
    public class Xmp : IModule
    {
        private readonly bool _skipElementOnMissingData;
        private readonly bool _errorOnDoubleKeys;
        private readonly bool _delocalizing;
        private readonly bool _flatting;
        private readonly List<XmpSearchEntry> toSearch = new List<XmpSearchEntry>();
        private readonly Dictionary<string, string> namespaceAlias = new Dictionary<string, string>();


        public Xmp(bool skipElementOnMissingMandatoryData = false, bool errorsOnDoubleKeys = true, bool delocalizing = true, bool flatten = true)
        {
            _skipElementOnMissingData = skipElementOnMissingMandatoryData;
            _errorOnDoubleKeys = errorsOnDoubleKeys;
            _delocalizing = delocalizing;
            _flatting = flatten;

            namespaceAlias["dc"] = "http://purl.org/dc/elements/1.1/";
            namespaceAlias["xmpRights"] = "http://ns.adobe.com/xap/1.0/rights/";
            namespaceAlias["cc"] = "http://creativecommons.org/ns#";
            namespaceAlias["xmp"] = "http://ns.adobe.com/xap/1.0/";
            namespaceAlias["xml"] = "http://www.w3.org/XML/1998/namespace";
        }


        public Xmp WithMetadata(string xmpPath, string targetMetadata, bool isMandatory = false)
        {
            this.toSearch.Add(new XmpSearchEntry(this, isMandatory, targetMetadata, xmpPath));
            return this;
        }

        public Xmp WithNamespace(string xmlNamespace, string alias)
        {
            namespaceAlias[alias] = xmlNamespace;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select(x =>
             {
                 MetadataExtractor.Formats.Xmp.XmpDirectory directories2;
                 try
                 {
                     directories2 = ImageMetadataReader.ReadMetadata(x.Source).OfType<MetadataExtractor.Formats.Xmp.XmpDirectory>().FirstOrDefault();
                 }
                 catch (Exception)
                 {
                     directories2 = null;
                 }
                 if (directories2 == null) // Try to read sidecarfile
                 {
                     if (System.IO.File.Exists(x.Source + ".xmp"))
                     {
                         var xmpXml = System.IO.File.ReadAllText(x.Source + ".xmp");
                         directories2 = new MetadataExtractor.Formats.Xmp.XmpReader().Extract(xmpXml);
                     }
                 }
                 if (directories2 == null)
                 {
                     if (toSearch.Any(y => y.IsMandatory))
                     {
                         context.Trace.Warning($"File doe not contain Metadata or sidecar file ({x.Source})");
                         if (_skipElementOnMissingData)
                             return null;
                     }
                     return x;
                 }

                 Dictionary<string, object> newValues = new Dictionary<string, object>();

                 var hirachciDirectory = TreeDirectory.GetHirachicDirectory(directories2);

                 foreach (var search in toSearch)
                 {
                     try
                     {
                         
                         var metadata = hirachciDirectory.Childrean.FirstOrDefault(y => search.PathWithoutNamespacePrefix == y.ElementName && search.Namespace == y.ElementNameSpace);

                         if (metadata == null)
                         {
                             if (search.IsMandatory)
                             {
                                 context.Trace.Error($"Metadata does not Contain {search.XmpPath} ({x.Source})");
                                 if (_skipElementOnMissingData)
                                     return null;
                             }
                             continue;
                         }
                         object value = GetObjectFromMetadata(metadata, hirachciDirectory);
                         if (newValues.ContainsKey(search.MetadataKey) && _errorOnDoubleKeys)
                         {
                             context.Trace.Error($"This Module tries to write same Key multiple times {search.MetadataKey} ({x.Source})");
                         }
                         else
                         {
                             newValues[search.MetadataKey] = value;
                         }

                     }
                     catch (Exception e)
                     {
                         context.Trace.Error($"An Exception Occured: {e} {e.Message}");
                         if (search.IsMandatory && _skipElementOnMissingData)
                             return null;
                     }
                 }
                 if (newValues.Any())
                     return x.Clone(newValues);
                 return x;
             }).Where(x => x != null);
        }


        [System.Diagnostics.DebuggerDisplay("{ElementName}: {ElementValue} [{ElementArrayIndex}] ({ElementNameSpace})")]
        private class TreeDirectory
        {

            public string ElementName
            {
                get
                {
                    string path = Element?.Path;
                    if (string.IsNullOrWhiteSpace(path))
                        return null;
                    string pathWithouParent;
                    if (!string.IsNullOrWhiteSpace(Parent?.Element?.Path))
                        pathWithouParent = path.Substring(Parent.Element.Path.Length).TrimStart('/');
                    else
                        pathWithouParent = path.TrimStart('/');
                    string pathWithoutNamespace = Regex.Replace(pathWithouParent, @"^[^:]+:(?<tag>[^/]+)(/.*)?$", "${tag}");

                    if (Regex.IsMatch(pathWithoutNamespace, @"\[\d+\]"))
                        return null;
                    return pathWithoutNamespace;
                }
            }
            public int ElementArrayIndex

            {
                get
                {
                    string path = Element?.Path;
                    if (string.IsNullOrWhiteSpace(path))
                        return -1;

                    string pathWithouParent;
                    if (!string.IsNullOrWhiteSpace(Parent?.Element?.Path))
                        pathWithouParent = path.Substring(Parent.Element.Path.Length).TrimStart('/');
                    else
                        pathWithouParent = path.TrimStart('/');
                    string pathWithoutNamespace = Regex.Replace(pathWithouParent, @"^[^:]+:(?<tag>[^/]+)(/.*)?$", "${tag}");

                    if (Regex.IsMatch(pathWithoutNamespace, @"\[\d+\]"))
                        return int.Parse(Regex.Replace(pathWithoutNamespace, @"\[(?<index>\d+)\]", "${index}"));
                    return -1;
                }
            }
            public bool IsArrayElement => this.ElementArrayIndex != -1;
            public string ElementNameSpace => Element?.Namespace;
            public string ElementValue => Element?.Value;
            public IXmpPropertyInfo Element { get; }
            public List<TreeDirectory> Childrean { get; } = new List<TreeDirectory>();
            public TreeDirectory Parent { get; private set; }

            private TreeDirectory()
            {

            }

            private TreeDirectory(IXmpPropertyInfo x)
            {
                this.Element = x;
            }

            internal static TreeDirectory GetHirachicDirectory(XmpDirectory directories)
            {
                var root = new TreeDirectory();

                var treeNodes = directories.XmpMeta.Properties.Where(x => x.Path != null).Select(x => new TreeDirectory(x)).ToArray();

                var possibleChildrean = treeNodes.Select(x => new
                {
                    Element = x,
                    PossibleChildrean = treeNodes.Where(y => y.Element.Path != x.Element.Path && y.Element.Path.StartsWith(x.Element.Path)).ToArray()
                }).ToArray();
                var childOfRoot = possibleChildrean.Where(x => !possibleChildrean.Any(y => y.PossibleChildrean.Contains(x.Element))).ToArray();

                root.Childrean.AddRange(childOfRoot.Select(x => x.Element));
                foreach (var child in childOfRoot)
                {
                    child.Element.Parent = root;
                }

                foreach (var node in possibleChildrean)
                {
                    var childOfNode = node.PossibleChildrean.Where(x => !possibleChildrean.Where(y => node.PossibleChildrean.Contains(y.Element)).Any(y => y.PossibleChildrean.Contains(x))).ToArray();

                    node.Element.Childrean.AddRange(childOfNode);
                    foreach (var child in childOfNode)
                    {
                        child.Parent = node.Element;
                    }

                }

                return root;
            }
        }

        private object GetObjectFromMetadata(TreeDirectory metadata, TreeDirectory hirachciDirectory)
        {
            if (metadata.Element.Options.IsArray)
            {
                var arreyElemnts = metadata.Childrean.Where(x => x.IsArrayElement).OrderBy(x => x.ElementArrayIndex);
                var array = arreyElemnts.Select(y => GetObjectFromMetadata(y, hirachciDirectory)).ToArray();
                if (array.All(x => x is LocalizedString))
                {
                    CultureInfo systemCulture = System.Globalization.CultureInfo.CurrentCulture;
                    LocalizedString matchingString = null;
                    do
                    {
                        matchingString = array.OfType<LocalizedString>().FirstOrDefault(x => x.Culture.Equals(systemCulture));
                        if (systemCulture.Parent.Equals(systemCulture))
                            break; // We are at the Culture Root. so break or run for ever.
                        systemCulture = systemCulture.Parent;

                    } while (matchingString == null);

                    if (matchingString != null)
                        return matchingString.Value;
                }
                if (_flatting && array.Length == 1)
                    return array[0];
                return array;
            }
            else if (metadata.Element.Options.IsStruct)
            {
                IDictionary<string, object> obj = new System.Dynamic.ExpandoObject();
                var properties = metadata.Childrean;// directories.XmpMeta.Properties.Where(x => x.Path != null && x.Path.StartsWith(metadata.Path))
                    
                foreach (var prop in properties)
                {
                    obj.Add(prop.ElementName, GetObjectFromMetadata(prop, hirachciDirectory));
                }
                return obj;
            }
            else if (metadata.Element.Options.IsSimple)
            {
                //xml:lang, de

                if (metadata.Element.Options.HasLanguage)
                {
                    var langMetadata =  metadata.Childrean.Single(x=>x.ElementName=="lang" && x.ElementNameSpace == "http://www.w3.org/XML/1998/namespace");
                    System.Globalization.CultureInfo culture;
                    if (langMetadata.ElementValue == "x-default")
                    {
                        culture = System.Globalization.CultureInfo.InvariantCulture;
                    }
                    else
                    {
                        culture = System.Globalization.CultureInfo.GetCultureInfo(langMetadata.ElementValue);
                    }

                    return new LocalizedString() { Culture = culture, Value = metadata.ElementValue };

                }

                return metadata.ElementValue;
            }
            else
            {
                throw new NotSupportedException($"Option {metadata.Element.Options.GetOptionsString()} not supported.");
            }


        }

        private class LocalizedString
        {
            public string Value { get; set; }
            public System.Globalization.CultureInfo Culture { get; set; }

            public override string ToString()
            {
                return Value;
            }

            public static implicit operator string (LocalizedString localizedString)
            {
                return localizedString.Value;
            }
        }

        private class XmpSearchEntry
        {
            private readonly Xmp _parent;


            public XmpSearchEntry(Xmp parent, bool isMandatory, string targetMetadata, string xmpPath)
            {
                _parent = parent;
                this.IsMandatory = isMandatory;
                this.MetadataKey = targetMetadata;
                this.XmpPath = xmpPath;
                var alias = Regex.Replace(XmpPath, @"^(?<ns>[^:]+):(?<name>.+)$", "${ns}");
                if (!_parent.namespaceAlias.ContainsKey(alias))
                    throw new ArgumentException($"Namespace alias {alias} unknown.", nameof(xmpPath));
            }

            public string XmpPath { get; }

            public string PathWithoutNamespacePrefix => Regex.Replace(XmpPath, @"^(?<ns>[^:]+):(?<name>.+)$", "${name}");

            public string Namespace => _parent.namespaceAlias[Regex.Replace(XmpPath, @"^(?<ns>[^:]+):(?<name>.+)$", "${ns}")];

            public string MetadataKey { get; }
            public bool IsMandatory { get; }
        }
    }
}
