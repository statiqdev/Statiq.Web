using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.CodeAnalysis;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.SearchIndex;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Generates the API search index.
    /// </summary>
    public class ApiSearchIndex : Pipeline
    {
        internal ApiSearchIndex()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new If(ctx => ctx.Documents[Docs.Api].Any() && ctx.Bool(DocsKeys.SearchIndex),
                new Documents(Docs.Api),
                new Where((doc, ctx) => doc.String(CodeAnalysisKeys.Kind) == "NamedType"),
                new SearchIndex.SearchIndex((doc, ctx) =>
                        new SearchIndexItem(
                            ctx.GetLink(doc),
                            doc.String(CodeAnalysisKeys.DisplayName),
                            doc.String(CodeAnalysisKeys.DisplayName)
                        ))
                    .WithScript((scriptBuilder, context) =>
                    {
                        // Use a custom tokenizer that splits on camel case characters
                        // https://github.com/olivernn/lunr.js/issues/230#issuecomment-244790648
                        scriptBuilder.Insert(0, @"
var camelCaseTokenizer = function (obj) {
    var previous = '';
    return obj.toString().trim().split(/[\s\-]+|(?=[A-Z])/).reduce(function(acc, cur) {
        var current = cur.toLowerCase();
        if(acc.length === 0) {
            previous = current;
            return acc.concat(current);
        }
        previous = previous.concat(current);
        return acc.concat([current, previous]);
    }, []);
}
lunr.tokenizer.registerFunction(camelCaseTokenizer, 'camelCaseTokenizer')");
                        scriptBuilder.Replace("this.ref('id');", @"this.ref('id');
        this.tokenizer(camelCaseTokenizer);");
                        return scriptBuilder.ToString();
                    })
                    .WithPath("assets/js/searchIndex.js"),
                new WriteFiles()
            )
        };
    }
}
