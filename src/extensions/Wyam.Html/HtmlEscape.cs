using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Html
{
    /// <summary>
    /// Automatically escapes HTML content.
    /// </summary>
    /// <category>Content</category>
    public class HtmlEscape : IModule
    {
        private readonly Dictionary<char, string> _predefinedEscapeSequences;
        private readonly Dictionary<char, string> _currentlyEscapedCharacters = new Dictionary<char, string>();
        private readonly HashSet<char> _standardCharacters = new HashSet<char>();
        private bool _escapeAllNonStandardCharacters;

        /// <summary>
        /// Escapes HTML content with predefined escape sequences.
        /// </summary>
        public HtmlEscape()
        {
            _predefinedEscapeSequences = new Dictionary<char, string>();
            _predefinedEscapeSequences.Add('€', "&euro;");
            _predefinedEscapeSequences.Add(' ', "&nbsp;");
            _predefinedEscapeSequences.Add('"', "&quot;");
            _predefinedEscapeSequences.Add('&', "&amp;");
            _predefinedEscapeSequences.Add('<', "&lt;");
            _predefinedEscapeSequences.Add('>', "&gt;");
            _predefinedEscapeSequences.Add('\u00A0', "&nbsp;");
            _predefinedEscapeSequences.Add('¡', "&iexcl;");
            _predefinedEscapeSequences.Add('¢', "&cent;");
            _predefinedEscapeSequences.Add('£', "&pound;");
            _predefinedEscapeSequences.Add('¤', "&curren;");
            _predefinedEscapeSequences.Add('¥', "&yen;");
            _predefinedEscapeSequences.Add('¦', "&brvbar;");
            _predefinedEscapeSequences.Add('§', "&sect;");
            _predefinedEscapeSequences.Add('¨', "&uml;");
            _predefinedEscapeSequences.Add('©', "&copy;");
            _predefinedEscapeSequences.Add('ª', "&ordf;");
            _predefinedEscapeSequences.Add('¬', "&not;");
            _predefinedEscapeSequences.Add('­', "&shy;");
            _predefinedEscapeSequences.Add('®', "&reg;");
            _predefinedEscapeSequences.Add('¯', "&macr;");
            _predefinedEscapeSequences.Add('°', "&deg;");
            _predefinedEscapeSequences.Add('±', "&plusmn;");
            _predefinedEscapeSequences.Add('²', "&sup2;");
            _predefinedEscapeSequences.Add('³', "&sup3;");
            _predefinedEscapeSequences.Add('´', "&acute;");
            _predefinedEscapeSequences.Add('µ', "&micro;");
            _predefinedEscapeSequences.Add('¶', "&para;");
            _predefinedEscapeSequences.Add('·', "&middot;");
            _predefinedEscapeSequences.Add('¸', "&cedil;");
            _predefinedEscapeSequences.Add('¹', "&sup1;");
            _predefinedEscapeSequences.Add('º', "&ordm;");
            _predefinedEscapeSequences.Add('»', "&raquo;");
            _predefinedEscapeSequences.Add('¼', "&frac14;");
            _predefinedEscapeSequences.Add('½', "&frac12;");
            _predefinedEscapeSequences.Add('¾', "&frac34;");
            _predefinedEscapeSequences.Add('¿', "&iquest;");
            _predefinedEscapeSequences.Add('À', "&Agrave;");
            _predefinedEscapeSequences.Add('Á', "&Aacute;");
            _predefinedEscapeSequences.Add('Â', "&Acirc;");
            _predefinedEscapeSequences.Add('Ã', "&Atilde;");
            _predefinedEscapeSequences.Add('Ä', "&Auml;");
            _predefinedEscapeSequences.Add('Å', "&Aring;");
            _predefinedEscapeSequences.Add('Æ', "&AElig;");
            _predefinedEscapeSequences.Add('Ç', "&Ccedil;");
            _predefinedEscapeSequences.Add('È', "&Egrave;");
            _predefinedEscapeSequences.Add('É', "&Eacute;");
            _predefinedEscapeSequences.Add('Ê', "&Ecirc;");
            _predefinedEscapeSequences.Add('Ë', "&Euml;");
            _predefinedEscapeSequences.Add('Ì', "&Igrave;");
            _predefinedEscapeSequences.Add('Í', "&Iacute;");
            _predefinedEscapeSequences.Add('Î', "&Icirc;");
            _predefinedEscapeSequences.Add('Ï', "&Iuml;");
            _predefinedEscapeSequences.Add('Ð', "&ETH;");
            _predefinedEscapeSequences.Add('Ñ', "&Ntilde;");
            _predefinedEscapeSequences.Add('Ò', "&Ograve;");
            _predefinedEscapeSequences.Add('Ó', "&Oacute;");
            _predefinedEscapeSequences.Add('Ô', "&Ocirc;");
            _predefinedEscapeSequences.Add('Õ', "&Otilde;");
            _predefinedEscapeSequences.Add('Ö', "&Ouml;");
            _predefinedEscapeSequences.Add('×', "&times;");
            _predefinedEscapeSequences.Add('Ø', "&Oslash;");
            _predefinedEscapeSequences.Add('Ù', "&Ugrave;");
            _predefinedEscapeSequences.Add('Ú', "&Uacute;");
            _predefinedEscapeSequences.Add('Û', "&Ucirc;");
            _predefinedEscapeSequences.Add('Ü', "&Uuml;");
            _predefinedEscapeSequences.Add('Ý', "&Yacute;");
            _predefinedEscapeSequences.Add('Þ', "&THORN;");
            _predefinedEscapeSequences.Add('ß', "&szlig;");
            _predefinedEscapeSequences.Add('à', "&agrave;");
            _predefinedEscapeSequences.Add('á', "&aacute;");
            _predefinedEscapeSequences.Add('â', "&acirc;");
            _predefinedEscapeSequences.Add('ã', "&atilde;");
            _predefinedEscapeSequences.Add('ä', "&auml;");
            _predefinedEscapeSequences.Add('å', "&aring;");
            _predefinedEscapeSequences.Add('æ', "&aelig;");
            _predefinedEscapeSequences.Add('ç', "&ccedil;");
            _predefinedEscapeSequences.Add('è', "&egrave;");
            _predefinedEscapeSequences.Add('é', "&eacute;");
            _predefinedEscapeSequences.Add('ê', "&ecirc;");
            _predefinedEscapeSequences.Add('ë', "&euml;");
            _predefinedEscapeSequences.Add('ì', "&igrave;");
            _predefinedEscapeSequences.Add('í', "&iacute;");
            _predefinedEscapeSequences.Add('î', "&icirc;");
            _predefinedEscapeSequences.Add('ï', "&iuml;");
            _predefinedEscapeSequences.Add('ð', "&eth;");
            _predefinedEscapeSequences.Add('ñ', "&ntilde;");
            _predefinedEscapeSequences.Add('ò', "&ograve;");
            _predefinedEscapeSequences.Add('ó', "&oacute;");
            _predefinedEscapeSequences.Add('ô', "&ocirc;");
            _predefinedEscapeSequences.Add('õ', "&otilde;");
            _predefinedEscapeSequences.Add('ö', "&ouml;");
            _predefinedEscapeSequences.Add('÷', "&divide;");
            _predefinedEscapeSequences.Add('ø', "&oslash;");
            _predefinedEscapeSequences.Add('ù', "&ugrave;");
            _predefinedEscapeSequences.Add('ú', "&uacute;");
            _predefinedEscapeSequences.Add('û', "&ucirc;");
            _predefinedEscapeSequences.Add('ü', "&uuml;");
            _predefinedEscapeSequences.Add('ý', "&yacute;");
            _predefinedEscapeSequences.Add('þ', "&thorn;");
        }

        /// <summary>
        /// Defines a standard set of characters as 0-9, a-z, A-Z, newlines, and space. Use with
        /// the <c>EscapeAllNonstandard()</c> method to whitelist this default set of characters.
        /// </summary>
        public HtmlEscape WithDefaultStandard()
        {
            for (char c = '0'; c <= '9'; c++)
                _standardCharacters.Add(c);
            for (char c = 'a'; c <= 'z'; c++)
                _standardCharacters.Add(c);
            for (char c = 'A'; c <= 'Z'; c++)
                _standardCharacters.Add(c);
            _standardCharacters.Add('\r');
            _standardCharacters.Add('\n');
            _standardCharacters.Add(' ');
            return this;
        }

        /// <summary>
        /// Defines a custom set of standard characters to use with the <c>EscapeAllNonstandard()</c> method.
        /// </summary>
        /// <param name="standard">The standard set of characters to use.</param>
        public HtmlEscape WithStandard(params char[] standard)
        {
            foreach (char c in standard)
                _standardCharacters.Add(c);
            return this;
        }

        /// <summary>
        /// Escapes all nonstandard characters (standard characters are defined with the <c>WithDefaultStandard()</c>
        /// or <c>WithStandard()</c> methods).
        /// </summary>
        public HtmlEscape EscapeAllNonstandard()
        {
            _escapeAllNonStandardCharacters = true;
            return this;
        }

        /// <summary>
        /// Defines additional characters to escape.
        /// </summary>
        /// <param name="toEscape">The additional characters to escape.</param>
        public HtmlEscape WithEscapedChar(params char[] toEscape)
        {
            foreach (char c in toEscape)
            {
                _currentlyEscapedCharacters.Add(c,
                    _predefinedEscapeSequences.ContainsKey(c) ? _predefinedEscapeSequences[c] : GenerateEscape(c));
            }
            return this;
        }

        private string GenerateEscape(char c)
        {
            return $"&#{(int)c};";
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(context, input =>
            {
                string oldContent = input.Content;
                StringWriter outputString = new StringWriter();
                bool escaped = false;

                foreach (char c in oldContent)
                {
                    if (_escapeAllNonStandardCharacters)
                    {
                        if (_standardCharacters.Contains(c))
                        {
                            outputString.Write(c);
                        }
                        else if (_predefinedEscapeSequences.ContainsKey(c))
                        {
                            outputString.Write(_predefinedEscapeSequences[c]);
                            escaped = true;
                        }
                        else
                        {
                            outputString.Write(GenerateEscape(c));
                            escaped = true;
                        }
                    }
                    else
                    {
                        if (_currentlyEscapedCharacters.ContainsKey(c))
                        {
                            outputString.Write(_currentlyEscapedCharacters[c]);
                            escaped = true;
                        }
                        else
                        {
                            outputString.Write(c);
                        }
                    }
                }
                return escaped ? context.GetDocument(input, context.GetContentStream(outputString.ToString())) : input;
            });
        }
    }
}
