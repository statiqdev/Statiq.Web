using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace Wyam.CodeAnalysis.Analysis
{
    // Based on the corresponding class in Roslyn - can be removed once Roslyn makes this public
    internal abstract class XmlDocumentationProvider : DocumentationProvider
    {
        private readonly NonReentrantLock _gate = new NonReentrantLock();
        private Dictionary<string, string> _docComments;

        /// <summary>
        /// Gets the source stream for the XML document.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected abstract Stream GetSourceStream(CancellationToken cancellationToken);

        /// <summary>
        /// Creates an <see cref="XmlDocumentationProvider"/> from bytes representing XML documentation data.
        /// </summary>
        /// <param name="xmlDocCommentBytes">The XML document bytes.</param>
        /// <returns>An <see cref="XmlDocumentationProvider"/>.</returns>
        public static XmlDocumentationProvider CreateFromBytes(byte[] xmlDocCommentBytes)
        {
            return new ContentBasedXmlDocumentationProvider(xmlDocCommentBytes);
        }

        /// <summary>
        /// Creates an <see cref="XmlDocumentationProvider"/> from an XML documentation file.
        /// </summary>
        /// <param name="xmlDocCommentFilePath">The path to the XML file.</param>
        /// <returns>An <see cref="XmlDocumentationProvider"/>.</returns>
        public static XmlDocumentationProvider CreateFromFile(string xmlDocCommentFilePath)
        {
            return new FileBasedXmlDocumentationProvider(xmlDocCommentFilePath);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.FxCop.Rules.Security.Xml.SecurityXmlRules", "CA3053:UseXmlSecureResolver",
             MessageId = "System.Xml.XmlReader.Create",
             Justification = @"For the call to XmlReader.Create() below, CA3053 recommends setting the
XmlReaderSettings.XmlResolver property to either null or an instance of XmlSecureResolver.
However, the said XmlResolver property no longer exists in .NET portable framework (i.e. core framework) which means there is no way to set it.
So we suppress this error until the reporting for CA3053 has been updated to account for .NET portable framework.")]
        private XDocument GetXDocument(CancellationToken cancellationToken)
        {
            using (var stream = GetSourceStream(cancellationToken))
            using (var xmlReader = XmlReader.Create(stream, s_xmlSettings))
            {
                return XDocument.Load(xmlReader);
            }
        }

        protected override string GetDocumentationForSymbol(string documentationMemberID, CultureInfo preferredCulture, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_docComments == null)
            {
                using (_gate.DisposableWait(cancellationToken))
                {
                    try
                    {
                        _docComments = new Dictionary<string, string>();

                        XDocument doc = GetXDocument(cancellationToken);
                        foreach (var e in doc.Descendants("member"))
                        {
                            if (e.Attribute("name") != null)
                            {
                                using (var reader = e.CreateReader())
                                {
                                    reader.MoveToContent();
                                    _docComments[e.Attribute("name").Value] = reader.ReadInnerXml();
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            string docComment;
            return _docComments.TryGetValue(documentationMemberID, out docComment) ? docComment : "";
        }

        private static readonly XmlReaderSettings s_xmlSettings = new XmlReaderSettings()
        {
            DtdProcessing = DtdProcessing.Prohibit,
        };

        private sealed class ContentBasedXmlDocumentationProvider : XmlDocumentationProvider
        {
            private readonly byte[] _xmlDocCommentBytes;

            public ContentBasedXmlDocumentationProvider(byte[] xmlDocCommentBytes)
            {
                _xmlDocCommentBytes = xmlDocCommentBytes;
            }

            protected override Stream GetSourceStream(CancellationToken cancellationToken)
            {
                return new MemoryStream(_xmlDocCommentBytes);
            }

            public override bool Equals(object obj)
            {
                var other = obj as ContentBasedXmlDocumentationProvider;
                return other != null && EqualsHelper(other);
            }

            private bool EqualsHelper(ContentBasedXmlDocumentationProvider other)
            {
                // Check for reference equality first
                if (this == other || _xmlDocCommentBytes == other._xmlDocCommentBytes)
                {
                    return true;
                }

                // Compare byte sequences
                if (_xmlDocCommentBytes.Length != other._xmlDocCommentBytes.Length)
                {
                    return false;
                }

                for (int i = 0; i < _xmlDocCommentBytes.Length; i++)
                {
                    if (_xmlDocCommentBytes[i] != other._xmlDocCommentBytes[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                return CombineValues(_xmlDocCommentBytes);
            }

            private static int CombineValues<T>(T[] values, int maxItemsToHash = int.MaxValue)
            {
                if (values == null)
                {
                    return 0;
                }

                var maxSize = Math.Min(maxItemsToHash, values.Length);
                var hashCode = 0;

                for (int i = 0; i < maxSize; i++)
                {
                    T value = values[i];

                    // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                    if (value != null)
                    {
                        hashCode = Combine(value.GetHashCode(), hashCode);
                    }
                }

                return hashCode;
            }

            private static int Combine(int newKey, int currentKey)
            {
                return unchecked((currentKey * (int)0xA5555529) + newKey);
            }
        }

        private sealed class FileBasedXmlDocumentationProvider : XmlDocumentationProvider
        {
            private readonly string _filePath;

            public FileBasedXmlDocumentationProvider(string filePath)
            {
                _filePath = filePath;
            }

            protected override Stream GetSourceStream(CancellationToken cancellationToken)
            {
                return new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            }

            public override bool Equals(object obj)
            {
                var other = obj as FileBasedXmlDocumentationProvider;
                return other != null && _filePath == other._filePath;
            }

            public override int GetHashCode()
            {
                return _filePath.GetHashCode();
            }
        }
    }
}