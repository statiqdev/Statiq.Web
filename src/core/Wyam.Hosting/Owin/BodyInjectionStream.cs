using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wyam.Hosting.Owin
{
    /// Thanks to the project Glimpse :
    /// https://github.com/Glimpse/Glimpse/blob/version-2/source/Glimpse.Core/PreBodyTagInjectionStream.cs
    /// And to Owin.GoogleAnalytics:
    /// https://github.com/serbrech/Owin.GoogleAnalytics/blob/master/src/Owin.GoogleAnalytics/PreBodyTagInjectionStream.cs
    /// <summary>
    /// This class will inject some html snippet in the resulting HTML output.
    /// It will look for the last occurrence of the &lt;/body&gt; tag and inject the snippet right before that tag.
    /// An instance of this class should be assigned as a filter to the outgoing response so that the injection can be done once all the rendering is completed.
    /// </summary>
    internal class BodyInjectionStream : Stream
    {
        private const string BodyClosingTag = "</body>";

        private readonly string _htmlSnippet;
        private readonly Stream _outputStream;
        private readonly Encoding _contentEncoding;
        private readonly Regex _bodyEndRegex;

        private string _unwrittenCharactersFromPreviousCall;

        public BodyInjectionStream(string htmlSnippet, Stream outputStream, Encoding contentEncoding)
        {
            _htmlSnippet = htmlSnippet + BodyClosingTag;
            _outputStream = outputStream;
            _contentEncoding = contentEncoding;
            _bodyEndRegex = new Regex(BodyClosingTag, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
        }

        public override bool CanRead => _outputStream.CanRead;

        public override bool CanSeek => _outputStream.CanSeek;

        public override bool CanWrite => _outputStream.CanWrite;

        public override long Length => _outputStream.Length;

        public override long Position
        {
            get { return _outputStream.Position; }
            set { _outputStream.Position = value; }
        }

        public override void Close()
        {
            _outputStream.Close();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _outputStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _outputStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _outputStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // There are different cases we need to deal with
            // Normally you would expect the contentInBuffer to contain the complete HTML code to return, but this is not always true because it is possible that
            // the content that will be send back is larger than the buffer foreseen by ASP.NET (currently the buffer seems to be a little bit less than 16K)
            // and in that case this method will be called multiple times, which might result in false positives being written to the logs for not finding a </body>
            // in the current chunk.

            // So we need to be able to deal with the following cases without writing those false positives
            // 1 - the </body> tag is found
            // 2 - the </body> tag was not found because
            //      2.1 - the </body> tag will be available in one of the next calls because the total length of the output is larger than 16K
            //      2.2 - the </body> tag is split up between this buffer and the next e.g.: "</bo" en "dy>"
            //      2.3 - the </body> tag will never be available (is missing)
            //      2.4 - Multiple </body> tags are available of which some might be part of a Javascript string or the markup is badly formatted

            // The easiest way to deal with this is to look for the last match for the </body> tag and if it is found we write everything before it to the
            // output stream and keep that </body> tag and everything that follows it (normally only a </html> tag but it can also be a 2.4 case) for the next call.
            // In case there is no match for the </body> tag, then we write everything to the output stream except for the last 10 characters (normally the last 6 would suffice, but we take a little margin to reassure us somehow ;-)) which we keep until the next call.

            // If there is a next call, then we first prepend the characters we kept from the previous call to the content inside the buffer (which might complete a chunked </body> tag for instance)
            // and start our check all over again (which might result in finding a </body> tag or discarding a previously found </body> tag because that one was not the last one.
            // Anyhow, as long as we are not a the end and a </body> tag has been found previously, the output will be buffered, just to make sure there is no other </body> tag further down the stream.

            // If there is no next call, then the Flush method will be called and that one will deal with the current state, which means:
            // - in case there was a </body> tag found, the replacement will be done
            // - in case there was no </body> tag found, then the warning will be written to the log, indicating something went wrong
            // either way, the remaining unwritten characters will be sent down the output stream.

            string contentInBuffer = _contentEncoding.GetString(buffer, offset, count);

            // Prepend remaining characters from the previous call, if any
            if (!string.IsNullOrEmpty(_unwrittenCharactersFromPreviousCall))
            {
                contentInBuffer = _unwrittenCharactersFromPreviousCall + contentInBuffer;
                _unwrittenCharactersFromPreviousCall = null;
            }

            Match closingBodyTagMatch = _bodyEndRegex.Match(contentInBuffer);
            if (closingBodyTagMatch.Success)
            {
                // Hooray, we found "a" </body> tag, but that doesn't mean that this is "the" last </body> tag we are looking for

                // so we write everything before that match to the output stream
                WriteToOutputStream(contentInBuffer.Substring(0, closingBodyTagMatch.Index));

                // and keep the remainder for the next call or the Flush if there is no next call
                _unwrittenCharactersFromPreviousCall = contentInBuffer.Substring(closingBodyTagMatch.Index);
            }
            else
            {
                // there is no match found for </body> which could have different reasons like case 2.2 for instance
                // therefore we'll write everything except the last 10 characters to the output stream and we'll keep the last 10 characters for the next call or the Flush method
                if (contentInBuffer.Length <= 10)
                {
                    // the content has a maximum length of 10 characters, so we don't need to write anything to the output stream and we'll keep those
                    // characters for the next call (most likely the Flush)
                    _unwrittenCharactersFromPreviousCall = contentInBuffer;
                }
                else
                {
                    WriteToOutputStream(contentInBuffer.Substring(0, contentInBuffer.Length - 10));
                    _unwrittenCharactersFromPreviousCall = contentInBuffer.Substring(contentInBuffer.Length - 10);
                }
            }
        }

        public override void Flush()
        {
            if (!string.IsNullOrEmpty(_unwrittenCharactersFromPreviousCall))
            {
                string finalContentToWrite = _unwrittenCharactersFromPreviousCall;

                if (_bodyEndRegex.IsMatch(_unwrittenCharactersFromPreviousCall))
                {
                    // apparently we did seem to match a </body> tag, which means we can replace the last match with our HTML snippet
                    finalContentToWrite = _bodyEndRegex.Replace(_unwrittenCharactersFromPreviousCall, _htmlSnippet, 1);
                }

                // either way, if a replacement has been done or a warning has been written to the logs, the remaining unwritten characters must be written to the output stream
                WriteToOutputStream(finalContentToWrite);
            }

            _outputStream.Flush();
        }

        private void WriteToOutputStream(string content)
        {
            byte[] outputBuffer = _contentEncoding.GetBytes(content);
            _outputStream.Write(outputBuffer, 0, outputBuffer.Length);
        }
    }
}
