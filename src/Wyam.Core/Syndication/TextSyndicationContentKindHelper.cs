//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Wyam.Core.Syndication
{
    static class TextSyndicationContentKindHelper
    {
        public static bool IsDefined(TextSyndicationContentKind kind)
        {
            return (kind == TextSyndicationContentKind.Plaintext
                || kind == TextSyndicationContentKind.Html
                || kind == TextSyndicationContentKind.XHtml);
        }
    }
}
