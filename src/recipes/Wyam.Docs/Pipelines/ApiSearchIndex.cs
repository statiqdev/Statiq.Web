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
            new If(
                ctx => ctx.Documents[Docs.Api].Any() && ctx.Bool(DocsKeys.SearchIndex),
                new Documents(Docs.Api),
                new Where((doc, ctx) => doc.String(CodeAnalysisKeys.Kind) == "NamedType"),
                new SearchIndex.SearchIndex((doc, ctx) =>
                        new DocSearchIndexItem(
                            doc,
                            doc.String(CodeAnalysisKeys.DisplayName),
                            doc.String(CodeAnalysisKeys.DisplayName)
                        ))
                    .WithScript((scriptBuilder, context) =>
                    {
                        // Use a custom tokenizer that splits on camel case characters
                        // https://github.com/olivernn/lunr.js/issues/230#issuecomment-244790648
                        scriptBuilder.Insert(0, @"
var camelCaseTokenizer = function (builder) {

  var pipelineFunction = function (token) {
    var previous = '';
    // split camelCaseString to on each word and combined words
    // e.g. camelCaseTokenizer -> ['camel', 'case', 'camelcase', 'tokenizer', 'camelcasetokenizer']
    var tokenStrings = token.toString().trim().split(/[\s\-]+|(?=[A-Z])/).reduce(function(acc, cur) {
      var current = cur.toLowerCase();
      if (acc.length === 0) {
        previous = current;
        return acc.concat(current);
      }
      previous = previous.concat(current);
      return acc.concat([current, previous]);
    }, []);

    // return token for each string
    // will copy any metadata on input token
    return tokenStrings.map(function(tokenString) {
      return token.clone(function(str) {
        return tokenString;
      })
    });
  }

  lunr.Pipeline.registerFunction(pipelineFunction, 'camelCaseTokenizer')

  builder.pipeline.before(lunr.stemmer, pipelineFunction)
}");
                        scriptBuilder.Replace("this.ref('id');", @"this.ref('id');
        this.use(camelCaseTokenizer);");
                        return scriptBuilder.ToString();
                    })
                    .WithPath("assets/js/searchIndex.js"),
                new WriteFiles())
                .WithoutUnmatchedDocuments()
        };
    }
}
