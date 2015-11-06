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
                 MetadataExtractor.Formats.Xmp.XmpDirectory directories;
                 try
                 {
                     directories = ImageMetadataReader.ReadMetadata(x.Source).OfType<MetadataExtractor.Formats.Xmp.XmpDirectory>().FirstOrDefault();
                 }
                 catch (Exception)
                 {
                     directories = null;
                 }
                 if (directories == null) // Try to read sidecarfile
                 {
                     if (System.IO.File.Exists(x.Source + ".xmp"))
                     {
                         var xmpXml = System.IO.File.ReadAllText(x.Source + ".xmp");
                         directories = new MetadataExtractor.Formats.Xmp.XmpReader().Extract(xmpXml);
                     }
                 }
                 if (directories == null)
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

                 foreach (var search in toSearch)
                 {
                     try
                     {
                         var metadata = directories.XmpMeta.Properties.FirstOrDefault(y => search.XmpPath == y.Path);

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
                         object value = GetObjectFromMetadata(metadata, directories);
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

        private object GetObjectFromMetadata(IXmpPropertyInfo metadata, XmpDirectory directories)
        {
            if (metadata.Options.IsArray)
            {
                var arreyElemnts = directories.XmpMeta.Properties.Where(y => y.Path != null && Regex.IsMatch(y.Path, @"^" + metadata.Path + @"\[\d+\]$")).OrderBy(y => int.Parse(Regex.Replace(y.Path, @"^" + metadata.Path + @"\[(?<index>\d+)\]$", "${index}")));
                var array = arreyElemnts.Select(y => GetObjectFromMetadata(y, directories)).ToArray();
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
            else if (metadata.Options.IsStruct)
            {
                IDictionary<string, object> obj = new System.Dynamic.ExpandoObject();
                var properties = directories.XmpMeta.Properties.Where(x => x.Path != null && x.Path.StartsWith(metadata.Path))
                    .Select(x => new { XmpPropertyInfo = x, StructPropertyName = x.Path.Substring(metadata.Path.Length + 1) })
                    .Where(x => !x.StructPropertyName.Contains("/"))
                    .Select(x =>
                    {
                        int indexOfCollon = x.StructPropertyName.IndexOf(':');
                        if (indexOfCollon != -1)
                        {
                            return new { x.XmpPropertyInfo, StructPropertyName = x.StructPropertyName.Substring(indexOfCollon + 1) };
                        }
                        return x;
                    });
                foreach (var prop in properties)
                {
                    obj.Add(prop.StructPropertyName, GetObjectFromMetadata(prop.XmpPropertyInfo, directories));
                }
                return obj;
            }
            else if (metadata.Options.IsSimple)
            {
                //xml:lang, de

                if (metadata.Options.HasLanguage)
                {
                    var langMetadata = directories.XmpMeta.Properties.Single(y => y.Path == metadata.Path + "/xml:lang");
                    System.Globalization.CultureInfo culture;
                    if (langMetadata.Value == "x-default")
                    {
                        culture = System.Globalization.CultureInfo.InvariantCulture;
                    }
                    else
                    {
                        culture = System.Globalization.CultureInfo.GetCultureInfo(langMetadata.Value);
                    }

                    return new LocalizedString() { Culture = culture, Value = metadata.Value };

                }

                return metadata.Value;
            }
            else
            {
                throw new NotSupportedException($"Option {metadata.Options.GetOptionsString()} not supported.");
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

            public string PathWithoutNamespacePrefix => Regex.Replace(XmpPath, @"^(?<ns>[^:+]):(?<name>.+)$", "${name}");

            public string Namespace => _parent.namespaceAlias[Regex.Replace(XmpPath, @"^(?<ns>[^:+]):(?<name>.+)$", "${ns}")];

            public string MetadataKey { get; }
            public bool IsMandatory { get; }
        }
    }
}
