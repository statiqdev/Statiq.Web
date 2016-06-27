//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Xml;

namespace Wyam.Core.Syndication
{
    interface IExtensibleSyndicationObject
    {
        Dictionary<XmlQualifiedName, string> AttributeExtensions 
        { get; }
        SyndicationElementExtensionCollection ElementExtensions 
        { get; }
    }
}
